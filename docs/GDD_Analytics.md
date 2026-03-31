# GDD: Analytics

**Weekly progress (implementation):** [commit `a886db4` — level completion + time-per-level analytics](https://github.com/CSCI-526/main-assignment-ayce-every-week/commit/a886db434f792d8aab0fc8546764c17c8724f03d) (`feature/level-completion-and-time-analytics`). Update this link when you land a new milestone.

Death cause distribution is unchanged (see `DeathAnalyticsManager`, `death_log.csv`, lose-screen bars). This document adds **level completion rate** and **time per level** only.

---

## Level completion rate

**Data.** Each finished attempt appends one row to `session_outcomes.csv`: `outcome` is **Win** or **Loss**, plus level index, scene name, duration, timestamp.

**Rate.** Per scene: wins ÷ (wins + losses) over the play session (also shown on the end screen as “This level completion: X% (WW / LL)”).

**Charts.** `python scripts/visualize_analytics.py completion path/to/session_outcomes.csv -o completion_rate.png`  
Mockup: `python scripts/visualize_analytics.py all --mock -o docs/analytics_mockups` → `mock_completion_rate.png`

---

## Time per level

**Data.** Real-time seconds from `GameManager.Start` (`BeginLevelSession`) until win or loss—same CSV row as completion.

**Charts.** `python scripts/visualize_analytics.py duration path/to/session_outcomes.csv -o time_per_level.png` → `mock_time_per_level.png` with `--mock`.

**In-game.** End screen shows “Time this level: X.X s” for the last attempt.

---

## Export (WebGL / standalone)

End screen **Export analytics (CSV)** downloads or saves `death_log.csv` and `session_outcomes.csv` (two files).

---

## Summary

| Feature | File | Visualization |
|--------|------|----------------|
| Death causes (existing) | `death_log.csv` | In-game bars; `visualize_analytics.py death …` |
| Level completion rate | `session_outcomes.csv` | Bar chart; end-screen % |
| Time per level | `session_outcomes.csv` | Mean win vs loss duration; end-screen seconds |
