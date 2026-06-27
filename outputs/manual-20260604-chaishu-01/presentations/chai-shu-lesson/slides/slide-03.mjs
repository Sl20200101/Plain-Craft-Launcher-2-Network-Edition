import { C, addBackground, addPage, addTitle, splitDiagram, text } from "./theme.mjs";

export async function slide03(presentation, ctx) {
  const slide = presentation.slides.add();
  addBackground(ctx, slide);
  addTitle(ctx, slide, "再看两个例子", "每次都只拆成两个数。");

  splitDiagram(ctx, slide, { x: 126, y: 176, total: 4, left: 1, right: 3, accent: C.mintDark });
  splitDiagram(ctx, slide, { x: 710, y: 176, total: 5, left: 2, right: 3, accent: C.amberDark });
  text(ctx, slide, "4 可以分成 1 和 3", 126, 548, 424, 42, { size: 25, bold: true, align: "center", color: C.mintDark });
  text(ctx, slide, "5 可以分成 2 和 3", 710, 548, 424, 42, { size: 25, bold: true, align: "center", color: C.amberDark });
  addPage(ctx, slide, 3);
  return slide;
}
