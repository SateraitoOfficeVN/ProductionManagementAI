# harness-deck

Builds the 16:9 presentation of the ai/ harness, in English and Japanese, with ReportLab:

```
python scripts/harness-deck/deck.py docs/en/presentations/ai-harness-overview.pdf en
python scripts/harness-deck/deck.py docs/ja/presentations/ai-harness-overview.pdf ja
```

- `deck.py` draws all 14 slides. The slide text follows `ai/harness-overview.md`; when the overview changes, update the matching slide here too, then rebuild both editions.
- `ja_strings.py` maps every English string on the slides to its Japanese translation. The Japanese build stops with an error naming any string that has no entry, so a new or changed string can't silently stay in English.
- Needs `pip install reportlab` and the Windows fonts Segoe UI, Consolas and BIZ UDGothic (read from `C:/Windows/Fonts`).
