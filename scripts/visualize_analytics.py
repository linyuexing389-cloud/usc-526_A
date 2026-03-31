#!/usr/bin/env python3
"""
Generate charts from exported Unity analytics CSVs, or use --mock for demo / GDD mockups.

Examples:
  python scripts/visualize_analytics.py death path/to/death_log.csv -o out.png
  python scripts/visualize_analytics.py completion path/to/session_outcomes.csv
  python scripts/visualize_analytics.py duration path/to/session_outcomes.csv
  python scripts/visualize_analytics.py all --mock -o docs/analytics_mockups
"""
from __future__ import annotations

import argparse
import csv
import sys
from collections import defaultdict
from pathlib import Path


def _need_matplotlib():
    try:
        import matplotlib

        matplotlib.use("Agg")
        import matplotlib.pyplot as plt

        return plt
    except ImportError:
        print("Install matplotlib: pip install -r scripts/requirements-analytics.txt", file=sys.stderr)
        sys.exit(1)


def plot_death_causes(rows: list[dict], out: Path, plt) -> None:
    counts = {"Timeout": 0, "Spikes": 0, "Trap": 0}
    for r in rows:
        c = (r.get("deathCause") or "").strip()
        if c in counts:
            counts[c] += 1
    labels = list(counts.keys())
    vals = [counts[k] for k in labels]
    fig, ax = plt.subplots(figsize=(7, 4.5))
    colors = ["#e6a033", "#e64033", "#3380e6"]
    ax.bar(labels, vals, color=colors)
    ax.set_ylabel("Count")
    ax.set_title("Death Cause Distribution")
    fig.tight_layout()
    fig.savefig(out, dpi=150)
    plt.close(fig)


def plot_completion(rows: list[dict], out: Path, plt) -> None:
    # Per scene: wins / (wins + losses)
    stats: dict[str, tuple[int, int]] = defaultdict(lambda: (0, 0))
    for r in rows:
        scene = (r.get("sceneName") or "?").strip()
        o = (r.get("outcome") or "").strip().lower()
        w, l = stats[scene]
        if o == "win":
            stats[scene] = (w + 1, l)
        elif o == "loss":
            stats[scene] = (w, l + 1)
    scenes = sorted(stats.keys(), key=lambda s: stats[s][0] + stats[s][1], reverse=True)
    rates = []
    for s in scenes:
        w, lo = stats[s]
        tot = w + lo
        rates.append((w / tot * 100.0) if tot else 0.0)
    fig, ax = plt.subplots(figsize=(8, 4.5))
    x = range(len(scenes))
    ax.bar(x, rates, color="#4a9f6e")
    ax.set_xticks(list(x))
    ax.set_xticklabels(scenes, rotation=25, ha="right")
    ax.set_ylabel("Completion rate (%)")
    ax.set_ylim(0, 105)
    ax.set_title("Level completion rate (wins / attempts)")
    fig.tight_layout()
    fig.savefig(out, dpi=150)
    plt.close(fig)


def plot_duration(rows: list[dict], out: Path, plt) -> None:
    # Mean duration per scene, split Win vs Loss
    by_scene: dict[str, dict[str, list[float]]] = defaultdict(
        lambda: {"Win": [], "Loss": []}
    )
    for r in rows:
        scene = (r.get("sceneName") or "?").strip()
        o = (r.get("outcome") or "").strip()
        try:
            d = float(r.get("durationSec") or 0)
        except ValueError:
            continue
        if o in ("Win", "Loss"):
            by_scene[scene][o].append(d)
    scenes = sorted(by_scene.keys())
    win_means = []
    loss_means = []
    for s in scenes:
        w = by_scene[s]["Win"]
        lo = by_scene[s]["Loss"]
        win_means.append(sum(w) / len(w) if w else 0.0)
        loss_means.append(sum(lo) / len(lo) if lo else 0.0)
    fig, ax = plt.subplots(figsize=(8, 4.5))
    idx = range(len(scenes))
    w = 0.35
    ax.bar([i - w / 2 for i in idx], win_means, w, label="Win (mean sec)", color="#5cb85c")
    ax.bar([i + w / 2 for i in idx], loss_means, w, label="Loss (mean sec)", color="#d9534f")
    ax.set_xticks(list(idx))
    ax.set_xticklabels(scenes, rotation=25, ha="right")
    ax.set_ylabel("Seconds")
    ax.set_title("Mean time per level until outcome")
    ax.legend()
    fig.tight_layout()
    fig.savefig(out, dpi=150)
    plt.close(fig)


def read_csv(path: Path) -> list[dict]:
    if not path.is_file():
        return []
    with path.open(newline="", encoding="utf-8") as f:
        return list(csv.DictReader(f))


def mock_death() -> list[dict]:
    return [
        {"deathCause": "Timeout"},
        {"deathCause": "Timeout"},
        {"deathCause": "Spikes"},
        {"deathCause": "Spikes"},
        {"deathCause": "Spikes"},
        {"deathCause": "Spikes"},
        {"deathCause": "Trap"},
        {"deathCause": "Trap"},
        {"deathCause": "Trap"},
    ]


def mock_session() -> list[dict]:
    rows = []
    for scene, outcomes in [
        ("tutorial_level", ["Loss", "Loss", "Win"]),
        ("cube_map", ["Loss", "Win", "Win", "Loss", "Loss"]),
        ("cube_map 1", ["Loss", "Loss", "Loss", "Win"]),
    ]:
        for o in outcomes:
            dur = 45.0 if o == "Win" else 32.0
            rows.append(
                {
                    "level": "0",
                    "sceneName": scene,
                    "outcome": o,
                    "durationSec": str(dur + (hash(scene + o) % 20)),
                    "timestamp": "",
                }
            )
    return rows


def main() -> None:
    p = argparse.ArgumentParser(description="Unity beta analytics charts")
    p.add_argument(
        "metric",
        choices=["death", "completion", "duration", "all"],
        help="Which chart to build",
    )
    p.add_argument("csv_path", nargs="?", help="Input CSV (not needed with --mock)")
    p.add_argument("-o", "--output", default=".", help="Output file or directory")
    p.add_argument("--mock", action="store_true", help="Use simulated data (GDD mockups)")
    args = p.parse_args()
    plt = _need_matplotlib()

    out_base = Path(args.output)
    if args.metric == "all":
        out_dir = out_base if out_base.suffix == "" else out_base.parent
        out_dir.mkdir(parents=True, exist_ok=True)
        plot_death_causes(mock_death(), out_dir / "mock_death_causes.png", plt)
        plot_completion(mock_session(), out_dir / "mock_completion_rate.png", plt)
        plot_duration(mock_session(), out_dir / "mock_time_per_level.png", plt)
        print("Wrote:", out_dir)
        return

    if args.mock:
        data = {
            "death": mock_death(),
            "completion": mock_session(),
            "duration": mock_session(),
        }[args.metric]
    else:
        if not args.csv_path:
            p.error("csv_path required unless --mock")
        data = read_csv(Path(args.csv_path))

    name = {
        "death": "death_causes.png",
        "completion": "completion_rate.png",
        "duration": "time_per_level.png",
    }[args.metric]
    out = out_base if str(out_base).endswith(".png") else out_base / name
    out.parent.mkdir(parents=True, exist_ok=True)

    {
        "death": plot_death_causes,
        "completion": plot_completion,
        "duration": plot_duration,
    }[args.metric](data, out, plt)
    print("Wrote:", out)


if __name__ == "__main__":
    main()
