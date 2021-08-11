<!--
  This file is part of Flood, an interactive flood-fill visualizer.

  Copyright 2021 Eliah Kagan <degeneracypressure@gmail.com>

  Permission to use, copy, modify, and/or distribute this software for any
  purpose with or without fee is hereby granted.

  THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH
  REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY
  AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
  INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM
  LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR
  OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR
  PERFORMANCE OF THIS SOFTWARE.
-->

# Flood - interactive flood-fill visualizer

This program animates [flood fills](https://en.wikipedia.org/wiki/Flood_fill)
interactively, allowing the user to set the speed of each fill, the data
structure used to store coordinates of pixels that have been &ldquo;seen&rdquo;
but have not yet been filled, and the order in which neighboring pixels are
enumerated and added to this data structure.

Several choices of data structure are provided, including the usual options of
a stack (FIFO) or queue (LIFO). A recursively implemented flood fill is also
provided, in such a way that (like the iteratively implemented approaches) it
does not overflow the call stack even when used to fill a large region.

In effect, this is a rudimentary raster graphics editing canvas, whose
&ldquo;bucket tool&rdquo; operates slowly and without blocking other edits to
the canvas. As a flood fill proceeds, you can continue editing the canvas,
interacting with the fill&mdash;including by starting additional concurrent
flood fills.

When multiple concurrent flood fills are *nested*&mdash;with some working to
fill a region that others are trying to expand&mdash;they can interact and
interfere (&ldquo;fight&rdquo;) with each other. This is rarely relevant to
practical applications of flood fills, since flood fills are usually carried
out atomically rather than having their steps interleaved with other
operations. But it can produce visually interesting patterns that take a long
time to terminate. I hadn&rsquo;t anticipated that when I started working on
this program, but I now consider it to be the most interesting use of the
program, or at least the most fun.

If you use this program to do at most one fill per region at a time, with only
a stack or queue, and with only fixed-order neighbor enumeration, and you do
not otherwise modify the canvas, the results resemble the static animations
shown in the [Moving the recursion into a data
structure](https://en.wikipedia.org/wiki/Flood_fill#Moving_the_recursion_into_a_data_structure)
section of the [&ldquo;Flood fill&rdquo; on English Wikipedia]. I am grateful
to Finlay McWalter, who [made those
animations](https://en.wikipedia.org/w/index.php?title=Talk:Flood_fill&oldid=804243376#Large_scale_animation),
which inspired me to write this program. (I did not refer to McWalter&rsquo;s
source code in writing this, however.)

Unfortunately, this program only runs on Windows; I had originally intended
that it be a prototype for a nicer program that would be cross-platform, but I
ended up continuing to work on my &ldquo;prototype&rdquo; instead. I also have
not yet implemented videorecording, so it is not easy to save one&rsquo;s
animations (though it is possible to do so, using video capture software like
OBS). This is alpha 6 of the program; it has several [major bugs](BUGS.md) that
affect its usability.

The main goals of this program (besides being fun and looking cool) are to
demonstrate two concepts in computer science:

1. The flood fill algorithm: how it is really a graph traversal problem, and
   the way it is affected by choice of data structure and other related
   customizations.

2. Asynchronous programming in the context of responsive user-interface
   interaction, and the way language-level features for asynchronous
   programming (like `async`/`await` in C#, which this program uses) facilitate
   code whose structure clearly reflects the problem being solved, much more so
   than if one were to code up a state machine manually.
