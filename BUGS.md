<!--
  This file is part of Flood, an interactive flood-fill visualizer.

  Copyright (C) 2021 Eliah Kagan <degeneracypressure@gmail.com>

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

# Known Major Bugs

## Accessibility

### Colors cannot be customized.

Each kind of fill has a specific associated color, detailed in `doc/tips.html`
and `doc/help.html`. Flood should let the user customize these easily.

(The tips and full help are already customized with JavaScript; if the user
sets any non-default colors, they could reflect those when being accessed as
part of the program&rsquo;s UI, with the full help also mentioning the default
color.)

## UI/Features

### The UI is confusing.

People who&rsquo;ve tried out this program could not immediately tell how to
use most of its functionality.

This could be ameliorated by having a palette (accessible to the left of
&ldquo;Show Tips&rdquo; and/or always visible) of tools that provide specific
fills, and possibly also speed adjustments, as well as a default pointer tool
that has the current modifier-key-driven behavior.

### No videorecording.

Flood should provide a facility to record (appropriately compressed) video
files of fills, but I have not yet implemented that feature.

## Code Style

### The main source code file is too long and should be split up.

Flood is a LINQPad query and I had, originally, intended for the user to be
able to peruse most of the code in the left pane while interacting with the
application via the toolstrip and panels in the right pane.

Although ease of demonstration is sometimes a justification for cramming more
source code into a single file than one otherwise would, this is currently
excessive: the main source code file, `flood.linq`, is over 3500 lines. I
regard this situation to be a bug and I intend to split that file into several
files and/or make the code more easy to navigate in other ways.

(LINQPad fully supports queries spanning multiple files, so that&rsquo;s not
the issue.)

## Documentation

### The help and tips retrieve assets from the internet on each run.

Flood&rsquo;s built-in help retrieves fonts and JavaScript libraries from the
internet every time it runs, even though Flood is not itself a webapp. This was
convenient for development, but I do not regard it to be a good software
engineering practice for a desktop application.

### The &ldquo;full&rdquo; help is incomplete.

The full help file, `doc/help.html`, lacks some information that is present in
`README.md`.

### The technical explanation of `async` is confusing.

The technical explanation of how Flood uses `async`/`await` (which is used by
every fill except &ldquo;instant fill&rdquo;) is buried in a subsection of the
section on &ldquo;recursive fill&rdquo;. It is also hard to follow.

That material should be given its own top-level section and at least partly
rewritten.

## Stability

### The tab-management code breaks encapsulation.

As commented on `PanelSwitcher.TrySwitch`, the way program-driven switching
between LINQPad output panels is implemented is by circumventing encapsulation
on on the `OutputPanel` object (which runs in the query process, but is
supplied by a library, being present as a separate assembly from the query
assembly) and calling the `Activate` method.

The comment gives details about why I don&rdquo;t think any currently available
good means of doing this exist. But as long as I&rsquo;m doin that, Flood
cannot have a stable release, as `OutputPanel.Activate` has the `internal`
access modifier and it may be removed, or change behavior, in any future
version of LINQPad.

### The chart tooltip customization code breaks encapsulation.

The charting control doesn&rsquo;t support changing tooltip timings, and
I&rsquo;m circumventing encapsulation to work around that.

Unlike the tab-management encapsulation violation, this isn&rsquo;t necessarily
a huge big deal, since `System.Windows.Forms.DataVisualization` is no longer
maintained and unlikely to get updates, since this is not core program
functionality, and since I already degrade gracefully rather than crashing if
the attempt to access the tooltip object fails. *Arguably*, Flood could even
get a stable release without fixing this.

But it would be better to just fork the library (it&rsquo;s free open source
software) and add the functionality.
