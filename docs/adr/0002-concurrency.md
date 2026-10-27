# ADR-0002: Concurrent updates are detected with PostgreSQL xmin

## Status

Accepted, 24 October 2026

## Context

A leave balance is read, changed, and written back. Two requests handled at the same time can read the same row and write different results, and the second write silently discards the first. The same risk exists on a leave request when two approvers act on it at once.

The domain entities expose a single sanctioned path for each change, but nothing in the entity prevents two loaded copies from being saved one after the other.

## Options considered

1. **No concurrency token.** Last write wins. Any conflict is invisible.
2. **Application version column.** Add an integer column and a matching property, increment it on every save, and compare it on write.
3. **PostgreSQL system column `xmin`.** Use the row's transaction identifier as the token, mapped by the provider without a new column.

## Decision

We use `xmin` as the concurrency token for leave balances and leave requests.

`xmin` is a system column that every PostgreSQL row already has, and the provider can map it as a shadow property, so no Domain property and no extra column are added. When a save affects a row whose `xmin` changed since it was read, the update affects zero rows and the provider raises a concurrency exception.

The application translates that exception into a conflict response. Automatic retry is allowed only for operations that are idempotent by contract; anything that changes a decision is returned to the caller.

## Consequences

- The concurrency token is invisible in the Domain model. Tests that need to provoke a conflict must use two separate `DbContext` instances, not entity objects.
- Not every table is protected. Employees, positions, holidays, and job markers rely on their own guards (unique indexes and business pre-checks), not on `xmin`.
- `xmin` is tied to PostgreSQL. Moving to another database provider would require revisiting this decision.
- Automatic retry is safe only for idempotent operations. A decision such as approve or reject is never retried silently, because the second attempt might act on a state the first user never saw.
- A retried command must re-read the aggregate before saving again, otherwise it repeats the stale write.
- Conflicts surface as a distinct error type, so the API can answer with a conflict status instead of a generic failure.

## Related

The rules that a conflict must not violate are listed in ADR-0001, and the stored counters affected by a conflict are described in ADR-0003.
