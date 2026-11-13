================================================================
 GAME UI KIT: MENUS, HUD, DIALOGUE & FONTS  (v1.0)
 Pack 13 of a free pixel-art series - same 16x16 grid, style and 58-colour palette
================================================================

The whole game "flow" UI: title screen -> main menu -> settings -> save/load -> HUD while playing -> dialogue with the
pack 7 villagers -> shop -> pause -> game over / victory -> credits. Plus three real pixel FONTS for game engines.

HONEST COUNT (computed by code: md5 dedupe over every counted cell, empty cells ignored, rounded DOWN)
  1,559 drawn pieces & animation frames -> advertised as "1,500+"
      frames & windows 157, buttons & controls 481, screen parts 51,
      HUD 181, dialogue 91, shop/map/quests 101, input prompts 150,
      cursors 29, font glyphs 193, transitions & extras 125
      (298 of them are animation frames)
  listed separately (never in the headline):
      82 palette-swap pieces (same pixels as another piece, only other colours - e.g. a hover state that only lightens)
      86 near-duplicates (<= 3 px from another piece - e.g. small-font letters that differ by 2 px) - excluded
      28 pure 1:1 shifted frames (e.g. a bobbing arrow) and 26 mirrored pieces/glyphs (e.g. ) ] > } \ and the right journal page)
      174 recolor frames (theme colour variants, dark symbols, dark bubbles, extra digit/firework colours)
      141 accented font glyphs (base letter + accent mark), 72 bar/progress fill examples, 19 text-made pieces (banners, press start)
  No new colours were added. 17 example layouts (320x180) and 8 example screens (1280x720) are not counted.

THEMES (each a real design: own corner shape, rim, ornaments and textures)
  Wood       cozy farm      notched corners, iron brackets, nails, planks          variant: dark_wood
  Stone      dungeon        chamfered corners, bevelled blocks, chips, moss        variant: sandstone
  Parchment  quest / map    wobbly paper edge, dog-ear corners, ink, wax seal      variant: aged
  Dark       dark fantasy   spiked corners, steel plates, ruby rivets, purple trim variant: crimson
  Gold       royal shop     gold bevel, round bosses with rubies, navy velvet      variant: silver
  Pastel     cozy life-sim  rounded corners, flowers, candy shine                  variant: mint
  Colours per theme: Database/THEME_COLORS.json.  Variants: <theme>/recolors/.

FOLDERS
  01_Frames_Windows/<Theme>   panel 9-slice, inset panel, title bar, header ribbon, divider, tabs (active/inactive/hover),
                              scrollbar (track, thumb x3, arrows x3 states), close button x4 states, 2 corner ornaments (top-left;
                              flip in the engine), tooltip (+ tail), seamless 16x16 background tile
  02_Buttons_Controls/<Theme> buttons L/M/S + icon button in 5 states (normal, hover, pressed, disabled, focused), toggle (on/off x 3
                              states + slide animation), checkbox (off/on/partial), radio, slider (track, fill, knob x4), dropdown
                              (5 states + list + list items), text field (empty/focused/error/disabled + caret), stepper (-/+ x4 +
                              value box), keybind box (normal, listening 2f, conflict), progress bar (frame + fill + examples)
  02_Buttons_Controls/Icons   34 white 12x12 symbols for icon buttons (+ dark versions in recolors/)
  03_Screens                  logo frames (royal, wood, stone), press start, banners (GAME OVER, VICTORY!, PAUSED, LEVEL COMPLETE,
                              YOU WIN!, LOADING), victory stars (slot, full, pop 6f), save/load (slot cards, empty slot, thumbnail
                              frame, icons), credits scroll (paper + rollers), loading spinners, confirm icons,
                              Layouts/ = 17 example layouts at 320x180 built from the parts
  04_HUD                      hearts in 4 styles (full, half, empty, container upgrade, 1/4-3/4 pieces, beat 4f), bars in 4 styles x
                              health/stamina/mana (frame + fill + examples), currency (coin spin, gem sparkle, key, purse, counter
                              plates), HUD digit font (0-9 x , / : + - in gold/white/red/green), clock 8f, day/night dial 8f,
                              10 weather + 4 season indicators, minimap frames (round/square x gold/dark) + 12 markers, compass 8f +
                              heading strip, quest tracker, boss bar, interaction prompts + 10 action icons, toasts, XP bar + level
                              badge + level-up flash, hotbar (2 styles: slot, selected 4f, locked, empty, base)
  05_Dialogue                 per theme: text box, portrait frame (54x54, fits the pack 7 48x48 portraits), name plate, choice rows;
                              choice selectors, continue arrows, typing dots, speech bubbles (normal/whisper/shout/thought + tails),
                              14 animated emote bubbles (4f)
  06_Shop_Map_Quests          shop tabs, price tags, quantity selector, SOLD OUT stamp, item rows; world-map frame, compass rose, scale
                              bar, 16 map pins, player arrow (8 directions), 13 fog-of-war autotiles; quest cards, quest states,
                              journal pages, bookmark, page curl; achievement popup (+ shine 6f) + 24 achievement badges
  07_Input_Prompts            keyboard A-Z, 0-9, arrows, F1-F12, space/enter/shift/tab/esc/ctrl/alt/backspace (normal + pressed),
                              mouse (7), generic gamepad (letter face buttons, layout-neutral position buttons, d-pad, sticks,
                              bumpers, triggers, menu/view) - original generic designs, no brand logos or trademarks
  08_Cursors                  22 cursors + wait 8f (hotspots = "anchor" in the database)
  09_Fonts/<Font>             glyph sheets, BMFont .fnt + atlas .png, glyph map JSON, sample image (see FONTS)
  10_Transitions_Extras       8 full-screen transitions (320x180, 8f), fireworks, confetti, rating stars, check/cross marks,
                              arrows, text badges (NEW, SALE, HOT, MAX, LV, FREE), alert/count dots, flourishes, sparkle, glow ring
  Database                    ui_database.json/.csv (every piece: theme, file, size, 9-slice borders, states, frames, fps, anchor,
                              tileable, note), FONT_INFO.json, THEME_COLORS.json
  Example_Screens, Previews, Palette, LICENSE.txt

9-SLICE (borders are listed per piece as left,top,right,bottom in ui_database.csv -> column nine_slice)
  Corners never repeat in the stretch area: ornaments sit in the corners, edges are uniform along their length.
  Unity      Texture: Sprite (2D and UI), Filter Point, Compression None, Pixels Per Unit 16. Sprite Editor -> Border L/T/R/B.
             UI Image: Image Type "Sliced" (or "Tiled" for bg tiles). Use whole-number canvas scaling.
  Godot 4    NinePatchRect: patch_margin_left/top/right/bottom = the four numbers; axis_stretch Stretch (or Tile for bg tiles).
             Texture filter Nearest. For buttons use StyleBoxTexture with the same texture margins.
  GameMaker  Sprite Editor -> Nine Slice -> Enabled, set Left/Top/Right/Bottom; draw with draw_sprite_stretched().
  Tileable pieces (bg tiles, fog tiles, fills, list rows, transitions, credits paper, confetti) are marked tileable=true.

FONTS  (every glyph has an advance width and a common baseline; text-format BMFont, unicode=1)
  small    PixelSeries Small 5px          cap  5px  x-height  4px  line 11px  baseline  8  143 glyphs
  regular  PixelSeries Regular 7px        cap  7px  x-height  5px  line 14px  baseline 11  143 glyphs
  title    PixelSeries Title Bold 14px    cap 14px  x-height 10px  line 23px  baseline 18  143 glyphs
  Charset: ASCII 32-126, ß, and accented letters for Dutch, German, French and Spanish:
  ä ö ü ë ï ÿ é è ê à â á í ì î ó ò ô ú ù û ñ å ç  Ä Ö Ü Ë Ï É È Ê À Â Á Í Ó Ô Ú Û Ñ Î Å Ç  ¿ ¡
  Glyphs are white: tint them in the engine (vertex colour / modulate). Line height includes the room for accents.
  Godot 4    Drag <font>_bmfont.fnt into the project (imports as FontFile); Label -> Theme Overrides -> Font, size = native.
             Turn off antialiasing (Project Settings -> GUI -> Theme -> Default Font Antialiasing: None).
  Unity      TextMeshPro: Window -> TextMeshPro -> Font Asset Creator cannot read .fnt directly - use a free BMFont importer
             (e.g. a "BMFont to TMP" editor script) or create a TMP sprite asset from <font>_glyph_map.json; legacy UI Text: use
             a custom font asset with the character rects from the .fnt. Set the atlas to Point filter, no compression.
  GameMaker  font_add_sprite_ext(sprite, " !\"#...", proportional=true, sep=1) with the glyph sheet, or import the
             .fnt with a BMFont extension. Draw at the native size or whole multiples.
  Everything else: <font>_glyph_map.json gives x, y, w, h, yoffset and advance per character.

RESOLUTIONS
  Everything is drawn for 320x180 (scale 4x -> 1280x720, 6x -> 1920x1080) or 640x360 (2x / 3x). Use whole-number scaling only.
  Panels/buttons are on a 16px grid where it makes sense; bars and slots match the 16x16 icons of packs 2, 10 and 12.

COMBINE WITH THE SERIES
  Pack 7: portraits (48x48) sit inside the portrait frames at (3,3); villagers + emote bubbles above their heads.
  Pack 10: 16x16 item icons go into hotbar slots (1px inset), shop item rows (2,2) and save thumbnails; pack 10 keeps its
           inventory windows, rarity frames and tooltips - this pack adds the menus around them.
  Pack 11: the creator UI there; use these themes for the rest of the menus. Pack 12: skill bar + damage numbers stay in
           pack 12; this HUD adds hearts, stamina, money, time, map and quests around them. Pack 2: its 5x7 font is now a
           complete engine font here (regular), its hearts/bars got 4 new styles. Pack 5: new weather/season indicators.
  Pack 9: boss bar frame + minimap markers fit its bosses and monsters.

Made with AI assistance - programmatically generated pixel art, reviewed and curated.
See LICENSE.txt for the terms (same as packs 1-12).
