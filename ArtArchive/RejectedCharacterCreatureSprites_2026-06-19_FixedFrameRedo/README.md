Archived the rejected fixed-frame player and creature PNGs from commit a6da7fd.

This pass was closer to animation than the earlier still-image/squash sheets, but it
was rejected because the art direction still did not meet the target and every row
was forced into fixed six-frame-style output instead of using the frame count each
animation actually needed.

The active redo should use the sheet as the single source of animation frames. The
separate active idle PNGs were removed after this archive because idle already
lives inside row 0 of each full sheet.
