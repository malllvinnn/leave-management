# ADR-0003: Balance usage is stored as counters, not calculated on read

## Status

Accepted, 24 October 2026

## Context

A yearly leave balance is not a single number. Days are split into an annual quota and a carry-over portion, and each portion is either still available, reserved by a pending request, or already used. That gives four independent states per balance row: annual used, annual reserved, carry-over used, and carry-over reserved.

Every approval, rejection, cancellation, adjustment, and carry-over expiry changes some of those states.

## Options considered

1. **Calculate on read.** Store only the annual quota and the carried-over amount, then derive used and reserved by scanning requests and decisions.
2. **Store the four counters only.** Trust the counters as the single source of truth.
3. **Store the four counters and reconcile** them against the request data as a safety check.

## Decision

We store the four counters as integer columns and calculate totals from them. Totals are never stored as extra columns.

Used and reserved are derived properties computed from the four counters, so a save cannot leave a stored total disagreeing with its parts. A reconciliation check compares the counters against the allocation recorded on the requests for the same employee and year, and reports a mismatch instead of silently correcting it.

## Consequences

- Six `CHECK` constraints guard the counters: annual quota, carried over, annual used, annual reserved, carry-over used, carry-over reserved. None may be negative.
- Every mutation must update the counters inside the same transaction as the request change. A missing update corrupts the balance while leaving the request looking correct.
- Reading a balance is a single row lookup, which keeps the most frequent read cheap.
- A reconciliation job is needed to detect drift. It is deliberately not part of the write path, so a mismatch is visible rather than hidden.
- Carry-over expiry must decrement only the carry-over portion. Writing the expiry as a general adjustment would make the two portions indistinguishable.
