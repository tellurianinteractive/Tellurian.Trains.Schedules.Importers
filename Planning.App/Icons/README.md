# App icons

Source artwork for the icons served from `wwwroot`. These files are not part of the build; they exist
so the icon set can be regenerated consistently rather than hand-edited as pixels.

## Artwork and licence

The train glyph is the `train` symbol from [Material Design Icons](https://github.com/google/material-design-icons)
by Google, licensed under the [Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0).
The glyph path is unchanged; only the colour and the surrounding tile are ours. Attribution is not
required by the licence, but is recorded here so the provenance stays clear.

The tile uses `#03173d`, the same value as `theme_color` in the manifests.

## Files

| Source | Purpose | Rendered to |
| --- | --- | --- |
| `icon.svg` | Rounded tile | `wwwroot/icon-512.png`, `wwwroot/icon-192.png` |
| `icon-maskable.svg` | Full-bleed tile, glyph inside the 80% safe circle | `wwwroot/icon-maskable-512.png`, `wwwroot/icon-maskable-192.png` |
| `favicon.svg` | Tighter corners, larger glyph so it survives 32x32 | `wwwroot/favicon.svg` (copied as-is), `wwwroot/favicon.png` |

The manifests list the rounded tiles as `purpose: any` and the full-bleed ones as `purpose: maskable`,
so Android can crop to its own shape without clipping the train.

## Regenerating the PNGs

There is no image tooling in the repo; headless Edge renders the SVG and screenshots it, which keeps
the output identical to what a browser would draw. Point an HTML page at the SVG sized to N x N, then:

```
"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe" --headless --disable-gpu ^
  --force-device-scale-factor=1 --hide-scrollbars --default-background-color=00000000 ^
  --window-size=512,512 --screenshot=icon-512.png --virtual-time-budget=3000 file:///<page>.html
```

`--default-background-color=00000000` is what keeps the rounded corners transparent.

## If you change the glyph

The glyph is centred on its ink, not on its 24-unit view box: the `train` path occupies x 4..20 and
y 2..21, so the `translate` in each SVG offsets by that ink centre. A different glyph needs those
bounds recomputed, otherwise it will sit visibly low in the tile.
