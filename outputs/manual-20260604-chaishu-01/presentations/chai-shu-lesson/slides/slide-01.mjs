import { C, addBackground, addPage, splitDiagram, text } from "./theme.mjs";

export async function slide01(presentation, ctx) {
  const slide = presentation.slides.add();
  addBackground(ctx, slide, C.paper);

  text(ctx, slide, "拆数字", 88, 76, 420, 88, { size: 62, color: C.ink, bold: true });
  text(ctx, slide, "把一个数拆成两个数", 94, 174, 420, 44, { size: 26, color: C.muted });
  splitDiagram(ctx, slide, { x: 630, y: 112, total: 3, left: 1, right: 2, accent: C.blueDark });
  text(ctx, slide, "写法：数字下面画一撇一捺，像“八”字形。", 94, 418, 520, 42, {
    size: 27,
    bold: true,
    color: C.blueDark,
  });
  text(ctx, slide, "读法：3 可以分成 1 和 2。", 94, 494, 480, 42, { size: 27, color: C.ink });
  addPage(ctx, slide, 1);
  return slide;
}
