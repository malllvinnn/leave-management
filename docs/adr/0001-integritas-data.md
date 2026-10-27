# ADR-0001: Data integrity is enforced by the database and the application

## Status

Accepted, 24 October 2026

## Context

The domain rules describe states that must never exist: a negative balance, two active leave requests for the same employee covering the same date, a cycle in the manager chain, or an approval decision that changes after it was made. Today those rules exist in Domain entities and, from the next milestone, will also be called through the API.

The API is not the only writer. A migration, a manual `psql` session, an administrative script, or two API instances handling the same command can reach the same rows. Enforcement placed only in the application therefore protects the happy path and leaves every other path open.

## Options considered

1. **Application only.** Domain and handlers reject invalid data with readable messages. The database stores whatever it is given.
2. **Database only.** Constraints and triggers reject invalid writes. The application does not duplicate the checks and returns whatever error the database raises.
3. **Both, with different jobs.** The application produces business failures that a user can act on; the database is the final guard for every writer.

## Decision

We enforce integrity in both layers.

Unique rules are unique indexes: `lower(email)` on employees, `lower(name)` on positions, `(employee_id, year)` on leave balances, `(job_name, period_key)` on job execution markers, `(employee_id, operation, idempotency_key)` on idempotency records while the record is active, and `date` on holidays.

Balance rules are `CHECK` constraints on the stored counters: annual quota, carried over, annual used, annual reserved, carry-over used, and carry-over reserved are never negative.

Overlap is enforced by an exclusion constraint combining the employee with the request date range, restricted to the statuses that still occupy the calendar: `Pending`, `PendingFinalApproval`, and `Approved`.

History is protected by triggers that reject `UPDATE` and `DELETE` on approval decisions and audit entries, and by `ON DELETE RESTRICT` on every foreign key, so no parent deletion can remove a decision or an audit row.

The manager chain is guarded by a trigger on employees that walks the chain with a recursive query and rejects a cycle, and by a validator in the application that returns a business failure before the write is attempted.

Audit entries refer to their subject with a plain `entity_id` string and no foreign key. One column has to point at many tables, and audit is read per entity, never joined. Integrity of that history comes from the append-only triggers, not from a reference.

Only the unique indexes above are created up front. Any other index is added after `EXPLAIN ANALYZE` shows a real query needs it, so no index exists on a guess.

## Consequences

- Every rule change is a migration. Editing a constraint means writing a new migration, not editing C#.
- Database errors are not user-facing. The application must translate unique violations and exclusion violations into conflict responses, and keep its own pre-checks for the clear messages.
- Adding these constraints requires existing data to satisfy them. The initial migration runs on an empty database, so this is free now and expensive later.
- The exclusion constraint needs the `btree_gist` extension, which the deploying role must be allowed to create.
- Append-only protection means mistakes cannot be corrected by editing a row. A correction is a new row, which is the point of an audit trail.
- Tests that exercise repositories must expect database-level failures for direct writes that the application would have rejected earlier.

## Appendix: rule ownership

The `PR-nn` labels are only rule numbers for cross-reference; each row states the rule in full.

| Rule                                                                | Enforced by                                                  | Where                                |
| ------------------------------------------------------------------- | ------------------------------------------------------------ | ------------------------------------ |
| PR-01 employees.email unique, case-insensitive                      | unique index on `lower(email)`                               | EF configuration, initial migration  |
| PR-02 positions.name unique, case-insensitive                       | unique index on `lower(name)`                                | EF configuration, initial migration  |
| PR-03 (employee_id, year) unique                                    | unique index                                                 | EF configuration, initial migration  |
| PR-04 (job_name, period_key) unique                                 | unique index                                                 | EF configuration, initial migration  |
| PR-05 (employee_id, operation, idempotency_key) unique while active | partial unique index                                         | EF configuration, initial migration  |
| PR-06 holidays.date unique                                          | unique index                                                 | EF configuration, initial migration  |
| PR-07 balances never negative                                       | six `CHECK` constraints                                      | EF configuration, initial migration  |
| PR-08 manager chain has no cycle                                    | database trigger and `HierarchyValidator`                    | initial migration, application layer |
| PR-09 no overlapping active request                                 | exclusion constraint and repository pre-check                | initial migration, request pre-check |
| PR-10 decisions and audit are append-only                           | trigger on `UPDATE` and `DELETE`                             | initial migration                    |
| PR-11 timestamps stored in UTC                                      | `timestamptz` and a clock adapter that returns a zero offset | EF configuration, infrastructure     |
| PR-12 refresh token jti unique, revoked tokens unusable             | unique index and rotation logic                              | identity schema, token service       |
| PR-13 one application user per employee                             | unique index on `application_user_id`                        | identity schema                      |
| PR-14 concurrent updates are detected                               | `xmin` concurrency token                                     | see ADR-0002                         |
