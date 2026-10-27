# Architecture Decision Records

Each record explains one technical decision: the problem, the options that were considered, the choice, and what it costs.

| # | Record | Decision |
| --- | --- | --- |
| 0001 | [Data integrity](0001-integritas-data.md) | Integrity is enforced by unique indexes, `CHECK` constraints, an exclusion constraint, `ON DELETE RESTRICT`, and append-only triggers, with the application providing readable failures |
| 0002 | [Concurrency](0002-concurrency.md) | `xmin` is the concurrency token for balances and requests; conflicts become a conflict response |
| 0003 | [Balance counters](0003-bucket-saldo.md) | Four counters are stored, totals are calculated, and reconciliation reports drift instead of correcting it |

Records are numbered in order. A superseded record stays in place with its status changed; the one that replaces it gets the next number.
