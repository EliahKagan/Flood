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

# About Dependencies

Flood is free open source software. It is written by Eliah Kagan and
[**licensed**](LICENSE) under [0BSD](https://spdx.org/licenses/0BSD.html) (the
&ldquo;Zero-Clause BSD License&rdquo;). For convenience, that license is
reproduced here:


<details>
<summary>
<strong>View 0BSD</strong>&nbsp;&nbsp;&nbsp;
<em>&ldquo;Copyright 2020, 2021 Eliah Kagan&hellip;&rdquo;</em>
</summary>

> Copyright 2020, 2021 Eliah Kagan &lt;degeneracypressure@gmail.com&gt;
>
> Permission to use, copy, modify, and/or distribute this software for any
purpose with or without fee is hereby granted.
>
> THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH
REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY
AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM
LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR
OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR
PERFORMANCE OF THIS SOFTWARE.
</details>

However, it depends on some other software to run that is **written by other
authors** and **offered under different licenses** than 0BSD.

## Platform

Flood is a C# program targeting [.NET 5](https://dotnet.microsoft.com/) on
Windows. It uses [Windows Forms](https://github.com/dotnet/winforms), and to a
much lesser extent [WPF](https://github.com/dotnet/wpf), which are part of
.NET. It is implemented as a LINQPad 6 query;
[LINQPad](https://www.linqpad.net/) is a proprietary freeware application
written by Joseph Albahari. Flood&rsquo;s charting feature uses the version of
[System.Windows.Forms.DataVisualization](https://github.com/dotnet/winforms-datavisualization)
that is included in LINQPad 6 (which I believe is [this
fork](https://github.com/albahari/winforms-datavisualization)).

If the [Microsoft Edge WebView2
control](https://docs.microsoft.com/en-us/microsoft-edge/webview2/) is
installed, Flood&rsquo;s built-in help browser will use it automatically. If
not, it falls back to the [classic WebBrowser
control](https://docs.microsoft.com/en-us/previous-versions/windows/internet-explorer/ie-developer/platform-apis/aa752040(v=vs.85)).
(Also, the &ldquo;tips&rdquo; mini-help always uses the WebBrowser control.)
The help is written in such a way as to work properly via either engine, as
well as in any popular standalone web browsers such as Firefox or Chrome (for
perusal outside the application). Flood does not require WebView2 to run and
does not offer to install WebView2 or automate its installation.

Other than LINQPad&mdash;and Windows itself&mdash;the software Flood requires
to run is free and open source. See [Other Library
Dependencies](#Other-Library-Depenencies) and [Fonts](#Fonts) below.

## Other Library Dependencies

In addition to the components listed above, Flood uses these libraries:

- [classList.js](#classListjs) by Eli Grey
- [Deque](#Deque-NitoCollectionsDeque) by Stephen Cleary
- [Hyphenopoly](#Hyphenopoly) by Mathias Nater
- [Microsoft.Web.WebView2](#MicrosoftWebWebView2) from Microsoft Corporation
- [MoreLINQ](#MoreLINQ), maintained by Atif Aziz
- [URLSearchParams](#URLSearchParams) by Andrea Giammarchi

### classList.js

[classList.js](https://github.com/eligrey/classList.js/) 1.2.20180112 is
written by Eli Grey. [**It is
offered**](https://github.com/eligrey/classList.js/blob/1.2.20180112/LICENSE.md)
under the [Unlicense](https://unlicense.org/):

<details>
<summary>
<strong>View Unlicense</strong>&nbsp;&nbsp;&nbsp;
<em>&ldquo;This is free and unencumbered software released into the public domain.&hellip;&rdquo;</em>
</summary>

> This is free and unencumbered software released into the public domain.
>
> Anyone is free to copy, modify, publish, use, compile, sell, or
distribute this software, either in source code form or as a compiled
binary, for any purpose, commercial or non-commercial, and by any
means.
>
> In jurisdictions that recognize copyright laws, the author or authors
of this software dedicate any and all copyright interest in the
software to the public domain. We make this dedication for the benefit
of the public at large and to the detriment of our heirs and
successors. We intend this dedication to be an overt act of
relinquishment in perpetuity of all present and future rights to this
software under copyright law.
>
> THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
>
> For more information, please refer to <http://unlicense.org/>
</details>

### Deque (Nito.Collections.Deque)

[Deque](https://github.com/StephenCleary/Deque) 1.1.0 is written by Stephen
Cleary and [**licensed
under**](https://github.com/StephenCleary/Deque/blob/v1.1.0/LICENSE) the [MIT
license](https://spdx.org/licenses/MIT.html):

<details>
<summary>
<strong>View MIT license</strong>&nbsp;&nbsp;&nbsp;
<em>&ldquo;Copyright (c) 2015 Stephen Cleary&hellip;&rdquo;</em>
</summary>

> The MIT License (MIT)
>
> Copyright (c) 2015 Stephen Cleary
>
> Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
>
> The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
>
> THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
</details>

### Hyphenopoly

[Hyphenopoly](https://mnater.github.io/Hyphenopoly/) 3.4.0 is written by
Mathias Nater and [**licensed
under**](https://github.com/mnater/Hyphenopoly/blob/v3.4.0/LICENSE) the [MIT
license](https://spdx.org/licenses/MIT.html):

<details>
<summary>
<strong>View MIT license</strong>&nbsp;&nbsp;&nbsp;
<em>&ldquo;Copyright (c) 2019 Mathias Nater&hellip;&rdquo;</em>
</summary>

> The MIT License (MIT)
>
> Copyright (c) 2019 Mathias Nater
>
> Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
>
> The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
>
> THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
</details>

### Microsoft.Web.WebView2

[Microsoft.Web.WebView2](https://www.nuget.org/packages/Microsoft.Web.WebView2/1.0.902.49)
1.0.902.49 is written by software developers at Microsoft and [**licensed
under**](https://www.nuget.org/packages/Microsoft.Web.WebView2/1.0.902.49/License)
the following license (which is similar to the 3-clause BSD license):

<details>
<summary>
<strong>View license</strong>&nbsp;&nbsp;&nbsp;
<em>&ldquo;Copyright (C) Microsoft Corporation. All rights reserved.&hellip;&rdquo;</em>
</summary>

> Copyright (C) Microsoft Corporation. All rights reserved.
>
> Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are
met:
>
>   * Redistributions of source code must retain the above copyright
notice, this list of conditions and the following disclaimer.
>   * Redistributions in binary form must reproduce the above
copyright notice, this list of conditions and the following disclaimer
in the documentation and/or other materials provided with the
distribution.
>   * The name of Microsoft Corporation, or the names of its contributors
may not be used to endorse or promote products derived from this
software without specific prior written permission.
>
> THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
"AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

Microsoft.Web.WebView2 is a library that exposes the WebView2 control for use
by .NET programs, but it is not itself that control. This is to say
Microsoft.Web.WebView2 is only a *binding* for the WebView2 control. I do not
claim that the WebView2 control is itself available under the license shown
above, which I don&rsquo;t believe to be the case.

Flood does not require the WebView2 control, but it does require the
Microsoft.Web.WebView2 library even in the absence of the WebView2 control.
(The mechanism Flood uses to check if the WebView2 control is present uses that
library.)
</details>

### MoreLINQ

[MoreLINQ](https://morelinq.github.io/) 3.3.2 is written by various authors.
Its principal maintainer is [Atif Aziz](https://github.com/atifaziz); it has
[many contributors](https://github.com/morelinq/MoreLINQ/graphs/contributors).
It [**is licensed
under**](https://github.com/morelinq/MoreLINQ/blob/v3.3.2/COPYING.txt) the
[Apache 2.0 license](https://www.apache.org/licenses/LICENSE-2.0) with a small
portion of the code (from Microsoft) carrying the [MIT
license](https://spdx.org/licenses/MIT.html):

<details>
<summary>
<strong>View Apache License 2.0 + MIT license</strong>&nbsp;&nbsp;&nbsp;
<em>&ldquo;TERMS AND CONDITIONS FOR USE, REPRODUCTION, AND DISTRIBUTION&hellip;&rdquo;</em>
</summary>

> **Apache License**\
> **Version 2.0, January 2004**\
> **http://www.apache.org/licenses/**
>
>    TERMS AND CONDITIONS FOR USE, REPRODUCTION, AND DISTRIBUTION
>
>    1. Definitions.
>
>       "License" shall mean the terms and conditions for use, reproduction,
>       and distribution as defined by Sections 1 through 9 of this document.
>
>       "Licensor" shall mean the copyright owner or entity authorized by
>       the copyright owner that is granting the License.
>
>       "Legal Entity" shall mean the union of the acting entity and all
>       other entities that control, are controlled by, or are under common
>       control with that entity. For the purposes of this definition,
>       "control" means (i) the power, direct or indirect, to cause the
>       direction or management of such entity, whether by contract or
>       otherwise, or (ii) ownership of fifty percent (50%) or more of the
>       outstanding shares, or (iii) beneficial ownership of such entity.
>
>       "You" (or "Your") shall mean an individual or Legal Entity
>       exercising permissions granted by this License.
>
>       "Source" form shall mean the preferred form for making modifications,
>       including but not limited to software source code, documentation
>       source, and configuration files.
>
>       "Object" form shall mean any form resulting from mechanical
>       transformation or translation of a Source form, including but
>       not limited to compiled object code, generated documentation,
>       and conversions to other media types.
>
>       "Work" shall mean the work of authorship, whether in Source or
>       Object form, made available under the License, as indicated by a
>       copyright notice that is included in or attached to the work
>       (an example is provided in the Appendix below).
>
>       "Derivative Works" shall mean any work, whether in Source or Object
>       form, that is based on (or derived from) the Work and for which the
>       editorial revisions, annotations, elaborations, or other modifications
>       represent, as a whole, an original work of authorship. For the purposes
>       of this License, Derivative Works shall not include works that remain
>       separable from, or merely link (or bind by name) to the interfaces of,
>       the Work and Derivative Works thereof.
>
>       "Contribution" shall mean any work of authorship, including
>       the original version of the Work and any modifications or additions
>       to that Work or Derivative Works thereof, that is intentionally
>       submitted to Licensor for inclusion in the Work by the copyright owner
>       or by an individual or Legal Entity authorized to submit on behalf of
>       the copyright owner. For the purposes of this definition, "submitted"
>       means any form of electronic, verbal, or written communication sent
>       to the Licensor or its representatives, including but not limited to
>       communication on electronic mailing lists, source code control systems,
>       and issue tracking systems that are managed by, or on behalf of, the
>       Licensor for the purpose of discussing and improving the Work, but
>       excluding communication that is conspicuously marked or otherwise
>       designated in writing by the copyright owner as "Not a Contribution."
>
>       "Contributor" shall mean Licensor and any individual or Legal Entity
>       on behalf of whom a Contribution has been received by Licensor and
>       subsequently incorporated within the Work.
>
>    2. Grant of Copyright License. Subject to the terms and conditions of
>       this License, each Contributor hereby grants to You a perpetual,
>       worldwide, non-exclusive, no-charge, royalty-free, irrevocable
>       copyright license to reproduce, prepare Derivative Works of,
>       publicly display, publicly perform, sublicense, and distribute the
>       Work and such Derivative Works in Source or Object form.
>
>    3. Grant of Patent License. Subject to the terms and conditions of
>       this License, each Contributor hereby grants to You a perpetual,
>       worldwide, non-exclusive, no-charge, royalty-free, irrevocable
>       (except as stated in this section) patent license to make, have made,
>       use, offer to sell, sell, import, and otherwise transfer the Work,
>       where such license applies only to those patent claims licensable
>       by such Contributor that are necessarily infringed by their
>       Contribution(s) alone or by combination of their Contribution(s)
>       with the Work to which such Contribution(s) was submitted. If You
>       institute patent litigation against any entity (including a
>       cross-claim or counterclaim in a lawsuit) alleging that the Work
>       or a Contribution incorporated within the Work constitutes direct
>       or contributory patent infringement, then any patent licenses
>       granted to You under this License for that Work shall terminate
>       as of the date such litigation is filed.
>
>    4. Redistribution. You may reproduce and distribute copies of the
>       Work or Derivative Works thereof in any medium, with or without
>       modifications, and in Source or Object form, provided that You
>       meet the following conditions:
>
>       (a) You must give any other recipients of the Work or
>           Derivative Works a copy of this License; and
>
>       (b) You must cause any modified files to carry prominent notices
>           stating that You changed the files; and
>
>       (c) You must retain, in the Source form of any Derivative Works
>           that You distribute, all copyright, patent, trademark, and
>           attribution notices from the Source form of the Work,
>           excluding those notices that do not pertain to any part of
>           the Derivative Works; and
>
>       (d) If the Work includes a "NOTICE" text file as part of its
>           distribution, then any Derivative Works that You distribute must
>           include a readable copy of the attribution notices contained
>           within such NOTICE file, excluding those notices that do not
>           pertain to any part of the Derivative Works, in at least one
>           of the following places: within a NOTICE text file distributed
>           as part of the Derivative Works; within the Source form or
>           documentation, if provided along with the Derivative Works; or,
>           within a display generated by the Derivative Works, if and
>           wherever such third-party notices normally appear. The contents
>           of the NOTICE file are for informational purposes only and
>           do not modify the License. You may add Your own attribution
>           notices within Derivative Works that You distribute, alongside
>           or as an addendum to the NOTICE text from the Work, provided
>           that such additional attribution notices cannot be construed
>           as modifying the License.
>
>       You may add Your own copyright statement to Your modifications and
>       may provide additional or different license terms and conditions
>       for use, reproduction, or distribution of Your modifications, or
>       for any such Derivative Works as a whole, provided Your use,
>       reproduction, and distribution of the Work otherwise complies with
>       the conditions stated in this License.
>
>    5. Submission of Contributions. Unless You explicitly state otherwise,
>       any Contribution intentionally submitted for inclusion in the Work
>       by You to the Licensor shall be under the terms and conditions of
>       this License, without any additional terms or conditions.
>       Notwithstanding the above, nothing herein shall supersede or modify
>       the terms of any separate license agreement you may have executed
>       with Licensor regarding such Contributions.
>
>    6. Trademarks. This License does not grant permission to use the trade
>       names, trademarks, service marks, or product names of the Licensor,
>       except as required for reasonable and customary use in describing the
>       origin of the Work and reproducing the content of the NOTICE file.
>
>    7. Disclaimer of Warranty. Unless required by applicable law or
>       agreed to in writing, Licensor provides the Work (and each
>       Contributor provides its Contributions) on an "AS IS" BASIS,
>       WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
>       implied, including, without limitation, any warranties or conditions
>       of TITLE, NON-INFRINGEMENT, MERCHANTABILITY, or FITNESS FOR A
>       PARTICULAR PURPOSE. You are solely responsible for determining the
>       appropriateness of using or redistributing the Work and assume any
>       risks associated with Your exercise of permissions under this License.
>
>    8. Limitation of Liability. In no event and under no legal theory,
>       whether in tort (including negligence), contract, or otherwise,
>       unless required by applicable law (such as deliberate and grossly
>       negligent acts) or agreed to in writing, shall any Contributor be
>       liable to You for damages, including any direct, indirect, special,
>       incidental, or consequential damages of any character arising as a
>       result of this License or out of the use or inability to use the
>       Work (including but not limited to damages for loss of goodwill,
>       work stoppage, computer failure or malfunction, or any and all
>       other commercial damages or losses), even if such Contributor
>       has been advised of the possibility of such damages.
>
>    9. Accepting Warranty or Additional Liability. While redistributing
>       the Work or Derivative Works thereof, You may choose to offer,
>       and charge a fee for, acceptance of support, warranty, indemnity,
>       or other liability obligations and/or rights consistent with this
>       License. However, in accepting such obligations, You may act only
>       on Your own behalf and on Your sole responsibility, not on behalf
>       of any other Contributor, and only if You agree to indemnify,
>       defend, and hold each Contributor harmless for any liability
>       incurred by, or claims asserted against, such Contributor by reason
>       of your accepting any such warranty or additional liability.
>
>    END OF TERMS AND CONDITIONS
>
> ---
>
> The following notice applies to a small portion of the code:
>
> The MIT License (MIT)
>
> Copyright (c) Microsoft Corporation
>
> Permission is hereby granted, free of charge, to any person obtaining a copy
> of this software and associated documentation files (the "Software"), to deal
> in the Software without restriction, including without limitation the rights
> to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
> copies of the Software, and to permit persons to whom the Software is
> furnished to do so, subject to the following conditions:
>
> The above copyright notice and this permission notice shall be included in all
> copies or substantial portions of the Software.
>
> THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
> IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
> FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
> AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
> LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
> OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
> SOFTWARE.
</details>

### URLSearchParams

[URLSearchParams](https://github.com/ungap/url-search-params) 0.2.2 is written
by Andrea Giammarchi and [**licensed
under**](https://github.com/ungap/url-search-params/blob/v0.2.2/LICENSE) the
[ISC license](https://spdx.org/licenses/ISC.html):

<details>
<summary>
<strong>View ISC license</strong>&nbsp;&nbsp;&nbsp;
<em>&ldquo;Copyright (c) 2018, Andrea Giammarchi, @WebReflection&hellip;&rdquo;</em>
</summary>

> ISC License
>
> Copyright (c) 2018, Andrea Giammarchi, @WebReflection
>
> Permission to use, copy, modify, and/or distribute this software for any
purpose with or without fee is hereby granted, provided that the above
copyright notice and this permission notice appear in all copies.
>
> THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH
REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY
AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM
LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE
OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR
PERFORMANCE OF THIS SOFTWARE.
</details>

## Fonts

Aside from fonts supplied by the operating system, Flood uses these fonts:

- [IBM Plex Mono](#IBM-Plex-Mono)
- [Inter](#Inter)
- [Source Code Pro](#Source-Code-Pro)
- [Source Serif Pro](#Source-Serif-Pro)
- [Ubuntu](#Ubuntu)

### IBM Plex Mono

[IBM Plex Mono](https://www.ibm.com/plex/) is designed for IBM by Mike Abbink
and the [Bold Monday](https://boldmonday.com/custom/ibm/) team. It [**is
licensed**](https://github.com/IBM/plex/blob/master/LICENSE.txt) under the [SIL
OFL 1.1](https://scripts.sil.org/cms/scripts/page.php?site_id=nrsi&id=OFL):

<details>
<summary>
<strong>View SIL OFL 1.1</strong>&nbsp;&nbsp;&nbsp;
<em>&ldquo;Copyright © 2017 IBM Corp. with Reserved Font Name "Plex"&hellip;&rdquo;</em>
</summary>

> Copyright © 2017 IBM Corp. with Reserved Font Name "Plex"
>
> This Font Software is licensed under the SIL Open Font License, Version 1.1.
>
> This license is copied below, and is also available with a FAQ at: http://scripts.sil.org/OFL
>
>
> -----------------------------------------------------------\
> SIL OPEN FONT LICENSE Version 1.1 - 26 February 2007\
> \-----------------------------------------------------------
>
> PREAMBLE\
> The goals of the Open Font License (OFL) are to stimulate worldwide
> development of collaborative font projects, to support the font creation
> efforts of academic and linguistic communities, and to provide a free and
> open framework in which fonts may be shared and improved in partnership
> with others.
>
> The OFL allows the licensed fonts to be used, studied, modified and
> redistributed freely as long as they are not sold by themselves. The
> fonts, including any derivative works, can be bundled, embedded,
> redistributed and/or sold with any software provided that any reserved
> names are not used by derivative works. The fonts and derivatives,
> however, cannot be released under any other type of license. The
> requirement for fonts to remain under this license does not apply
> to any document created using the fonts or their derivatives.
>
> DEFINITIONS\
> "Font Software" refers to the set of files released by the Copyright
> Holder(s) under this license and clearly marked as such. This may
> include source files, build scripts and documentation.
>
> "Reserved Font Name" refers to any names specified as such after the
> copyright statement(s).
>
> "Original Version" refers to the collection of Font Software components as
> distributed by the Copyright Holder(s).
>
> "Modified Version" refers to any derivative made by adding to, deleting,
> or substituting -- in part or in whole -- any of the components of the
> Original Version, by changing formats or by porting the Font Software to a
> new environment.
>
> "Author" refers to any designer, engineer, programmer, technical
> writer or other person who contributed to the Font Software.
>
> PERMISSION & CONDITIONS\
> Permission is hereby granted, free of charge, to any person obtaining
> a copy of the Font Software, to use, study, copy, merge, embed, modify,
> redistribute, and sell modified and unmodified copies of the Font
> Software, subject to the following conditions:
>
> 1) Neither the Font Software nor any of its individual components,
> in Original or Modified Versions, may be sold by itself.
>
> 2) Original or Modified Versions of the Font Software may be bundled,
> redistributed and/or sold with any software, provided that each copy
> contains the above copyright notice and this license. These can be
> included either as stand-alone text files, human-readable headers or
> in the appropriate machine-readable metadata fields within text or
> binary files as long as those fields can be easily viewed by the user.
>
> 3) No Modified Version of the Font Software may use the Reserved Font
> Name(s) unless explicit written permission is granted by the corresponding
> Copyright Holder. This restriction only applies to the primary font name as
> presented to the users.
>
> 4) The name(s) of the Copyright Holder(s) or the Author(s) of the Font
> Software shall not be used to promote, endorse or advertise any
> Modified Version, except to acknowledge the contribution(s) of the
> Copyright Holder(s) and the Author(s) or with their explicit written
> permission.
>
> 5) The Font Software, modified or unmodified, in part or in whole,
> must be distributed entirely under this license, and must not be
> distributed under any other license. The requirement for fonts to
> remain under this license does not apply to any document created
> using the Font Software.
>
> TERMINATION\
> This license becomes null and void if any of the above conditions are
> not met.
>
> DISCLAIMER\
> THE FONT SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
> EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO ANY WARRANTIES OF
> MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT
> OF COPYRIGHT, PATENT, TRADEMARK, OR OTHER RIGHT. IN NO EVENT SHALL THE
> COPYRIGHT HOLDER BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
> INCLUDING ANY GENERAL, SPECIAL, INDIRECT, INCIDENTAL, OR CONSEQUENTIAL
> DAMAGES, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
> FROM, OUT OF THE USE OR INABILITY TO USE THE FONT SOFTWARE OR FROM
> OTHER DEALINGS IN THE FONT SOFTWARE.
</details>

### Inter

[Inter](https://github.com/rsms/inter/) is principally designed by Rasmus
Andersson. It [**is
licensed**](https://github.com/rsms/inter/blob/master/LICENSE.txt) under the
[SIL OFL
1.1](https://scripts.sil.org/cms/scripts/page.php?site_id=nrsi&id=OFL):

<details>
<summary>
<strong>View SIL OFL 1.1</strong>&nbsp;&nbsp;&nbsp;
<em>&ldquo;Copyright (c) 2016-2020 The Inter Project Authors&hellip;&rdquo;</em>
</summary>

> Copyright (c) 2016-2020 The Inter Project Authors.\
> "Inter" is trademark of Rasmus Andersson.\
> https://github.com/rsms/inter
>
> This Font Software is licensed under the SIL Open Font License, Version 1.1.\
> This license is copied below, and is also available with a FAQ at:\
> http://scripts.sil.org/OFL
>
> \-----------------------------------------------------------\
> SIL OPEN FONT LICENSE Version 1.1 - 26 February 2007\
> \-----------------------------------------------------------
>
> PREAMBLE\
> The goals of the Open Font License (OFL) are to stimulate worldwide
> development of collaborative font projects, to support the font creation
> efforts of academic and linguistic communities, and to provide a free and
> open framework in which fonts may be shared and improved in partnership
> with others.
>
> The OFL allows the licensed fonts to be used, studied, modified and
> redistributed freely as long as they are not sold by themselves. The
> fonts, including any derivative works, can be bundled, embedded,
> redistributed and/or sold with any software provided that any reserved
> names are not used by derivative works. The fonts and derivatives,
> however, cannot be released under any other type of license. The
> requirement for fonts to remain under this license does not apply
> to any document created using the fonts or their derivatives.
>
> DEFINITIONS\
> "Font Software" refers to the set of files released by the Copyright
> Holder(s) under this license and clearly marked as such. This may
> include source files, build scripts and documentation.
>
> "Reserved Font Name" refers to any names specified as such after the
> copyright statement(s).
>
> "Original Version" refers to the collection of Font Software components as
> distributed by the Copyright Holder(s).
>
> "Modified Version" refers to any derivative made by adding to, deleting,
> or substituting -- in part or in whole -- any of the components of the
> Original Version, by changing formats or by porting the Font Software to a
> new environment.
>
> "Author" refers to any designer, engineer, programmer, technical
> writer or other person who contributed to the Font Software.
>
> PERMISSION AND CONDITIONS\
> Permission is hereby granted, free of charge, to any person obtaining
> a copy of the Font Software, to use, study, copy, merge, embed, modify,
> redistribute, and sell modified and unmodified copies of the Font
> Software, subject to the following conditions:
>
> 1) Neither the Font Software nor any of its individual components,
> in Original or Modified Versions, may be sold by itself.
>
> 2) Original or Modified Versions of the Font Software may be bundled,
> redistributed and/or sold with any software, provided that each copy
> contains the above copyright notice and this license. These can be
> included either as stand-alone text files, human-readable headers or
> in the appropriate machine-readable metadata fields within text or
> binary files as long as those fields can be easily viewed by the user.
>
> 3) No Modified Version of the Font Software may use the Reserved Font
> Name(s) unless explicit written permission is granted by the corresponding
> Copyright Holder. This restriction only applies to the primary font name as
> presented to the users.
>
> 4) The name(s) of the Copyright Holder(s) or the Author(s) of the Font
> Software shall not be used to promote, endorse or advertise any
> Modified Version, except to acknowledge the contribution(s) of the
> Copyright Holder(s) and the Author(s) or with their explicit written
> permission.
>
> 5) The Font Software, modified or unmodified, in part or in whole,
> must be distributed entirely under this license, and must not be
> distributed under any other license. The requirement for fonts to
> remain under this license does not apply to any document created
> using the Font Software.
>
> TERMINATION\
> This license becomes null and void if any of the above conditions are
> not met.
>
> DISCLAIMER\
> THE FONT SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
> EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO ANY WARRANTIES OF
> MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT
> OF COPYRIGHT, PATENT, TRADEMARK, OR OTHER RIGHT. IN NO EVENT SHALL THE
> COPYRIGHT HOLDER BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
> INCLUDING ANY GENERAL, SPECIAL, INDIRECT, INCIDENTAL, OR CONSEQUENTIAL
> DAMAGES, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
> FROM, OUT OF THE USE OR INABILITY TO USE THE FONT SOFTWARE OR FROM
> OTHER DEALINGS IN THE FONT SOFTWARE.
</details>

### Source Code Pro

<details>
<summary>
<strong>View SIL OFL 1.1</strong>&nbsp;&nbsp;&nbsp;
<em>&ldquo;Copyright 2010-2019 Adobe (http://www.adobe.com/), with Reserved Font Name 'Source'&hellip;&rdquo;</em>
</summary>

[Source Code Pro](https://adobe-fonts.github.io/source-code-pro/) is
principally designed by Paul D. Hunt. It [**is
licensed**](https://github.com/adobe-fonts/source-code-pro/blob/release/LICENSE.md)
under the [SIL OFL
1.1](https://scripts.sil.org/cms/scripts/page.php?site_id=nrsi&id=OFL):

> Copyright 2010-2019 Adobe (http://www.adobe.com/), with Reserved Font Name 'Source'. All Rights Reserved. Source is a trademark of Adobe in the United States and/or other countries.
>
> This Font Software is licensed under the SIL Open Font License, Version 1.1.
>
> This license is copied below, and is also available with a FAQ at: http://scripts.sil.org/OFL
>
>
> -----------------------------------------------------------\
> SIL OPEN FONT LICENSE Version 1.1 - 26 February 2007\
> \-----------------------------------------------------------
>
> PREAMBLE\
> The goals of the Open Font License (OFL) are to stimulate worldwide
> development of collaborative font projects, to support the font creation
> efforts of academic and linguistic communities, and to provide a free and
> open framework in which fonts may be shared and improved in partnership
> with others.
>
> The OFL allows the licensed fonts to be used, studied, modified and
> redistributed freely as long as they are not sold by themselves. The
> fonts, including any derivative works, can be bundled, embedded,
> redistributed and/or sold with any software provided that any reserved
> names are not used by derivative works. The fonts and derivatives,
> however, cannot be released under any other type of license. The
> requirement for fonts to remain under this license does not apply
> to any document created using the fonts or their derivatives.
>
> DEFINITIONS\
> "Font Software" refers to the set of files released by the Copyright
> Holder(s) under this license and clearly marked as such. This may
> include source files, build scripts and documentation.
>
> "Reserved Font Name" refers to any names specified as such after the
> copyright statement(s).
>
> "Original Version" refers to the collection of Font Software components as
> distributed by the Copyright Holder(s).
>
> "Modified Version" refers to any derivative made by adding to, deleting,
> or substituting -- in part or in whole -- any of the components of the
> Original Version, by changing formats or by porting the Font Software to a
> new environment.
>
> "Author" refers to any designer, engineer, programmer, technical
> writer or other person who contributed to the Font Software.
>
> PERMISSION & CONDITIONS\
> Permission is hereby granted, free of charge, to any person obtaining
> a copy of the Font Software, to use, study, copy, merge, embed, modify,
> redistribute, and sell modified and unmodified copies of the Font
> Software, subject to the following conditions:
>
> 1) Neither the Font Software nor any of its individual components,
> in Original or Modified Versions, may be sold by itself.
>
> 2) Original or Modified Versions of the Font Software may be bundled,
> redistributed and/or sold with any software, provided that each copy
> contains the above copyright notice and this license. These can be
> included either as stand-alone text files, human-readable headers or
> in the appropriate machine-readable metadata fields within text or
> binary files as long as those fields can be easily viewed by the user.
>
> 3) No Modified Version of the Font Software may use the Reserved Font
> Name(s) unless explicit written permission is granted by the corresponding
> Copyright Holder. This restriction only applies to the primary font name as
> presented to the users.
>
> 4) The name(s) of the Copyright Holder(s) or the Author(s) of the Font
> Software shall not be used to promote, endorse or advertise any
> Modified Version, except to acknowledge the contribution(s) of the
> Copyright Holder(s) and the Author(s) or with their explicit written
> permission.
>
> 5) The Font Software, modified or unmodified, in part or in whole,
> must be distributed entirely under this license, and must not be
> distributed under any other license. The requirement for fonts to
> remain under this license does not apply to any document created
> using the Font Software.
>
> TERMINATION\
> This license becomes null and void if any of the above conditions are
> not met.
>
> DISCLAIMER\
> THE FONT SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
> EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO ANY WARRANTIES OF
> MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT
> OF COPYRIGHT, PATENT, TRADEMARK, OR OTHER RIGHT. IN NO EVENT SHALL THE
> COPYRIGHT HOLDER BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
> INCLUDING ANY GENERAL, SPECIAL, INDIRECT, INCIDENTAL, OR CONSEQUENTIAL
> DAMAGES, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
> FROM, OUT OF THE USE OR INABILITY TO USE THE FONT SOFTWARE OR FROM
> OTHER DEALINGS IN THE FONT SOFTWARE.
</details>

### Source Serif Pro

[Source Serif](https://adobe-fonts.github.io/source-serif/) is principally
designed by Frank Grießhammer. It [**is
licensed**](https://github.com/adobe-fonts/source-serif/blob/release/LICENSE.md)
under the [SIL OFL
1.1](https://scripts.sil.org/cms/scripts/page.php?site_id=nrsi&id=OFL):

<details>
<summary>
<strong>View SIL OFL 1.1</strong>&nbsp;&nbsp;&nbsp;
<em>&ldquo;Copyright 2014-2021 Adobe (http://www.adobe.com/), with Reserved Font Name 'Source'&hellip;&rdquo;</em>
</summary>

> Copyright 2014-2021 Adobe (http://www.adobe.com/), with Reserved Font Name 'Source'. All Rights Reserved. Source is a trademark of Adobe in the United States and/or other countries.
>
> This Font Software is licensed under the SIL Open Font License, Version 1.1.
>
> This license is copied below, and is also available with a FAQ at: http://scripts.sil.org/OFL
>
>
> -----------------------------------------------------------\
> SIL OPEN FONT LICENSE Version 1.1 - 26 February 2007\
> \-----------------------------------------------------------
>
> PREAMBLE\
> The goals of the Open Font License (OFL) are to stimulate worldwide
> development of collaborative font projects, to support the font creation
> efforts of academic and linguistic communities, and to provide a free and
> open framework in which fonts may be shared and improved in partnership
> with others.
>
> The OFL allows the licensed fonts to be used, studied, modified and
> redistributed freely as long as they are not sold by themselves. The
> fonts, including any derivative works, can be bundled, embedded,
> redistributed and/or sold with any software provided that any reserved
> names are not used by derivative works. The fonts and derivatives,
> however, cannot be released under any other type of license. The
> requirement for fonts to remain under this license does not apply
> to any document created using the fonts or their derivatives.
>
> DEFINITIONS\
> "Font Software" refers to the set of files released by the Copyright
> Holder(s) under this license and clearly marked as such. This may
> include source files, build scripts and documentation.
>
> "Reserved Font Name" refers to any names specified as such after the
> copyright statement(s).
>
> "Original Version" refers to the collection of Font Software components as
> distributed by the Copyright Holder(s).
>
> "Modified Version" refers to any derivative made by adding to, deleting,
> or substituting -- in part or in whole -- any of the components of the
> Original Version, by changing formats or by porting the Font Software to a
> new environment.
>
> "Author" refers to any designer, engineer, programmer, technical
> writer or other person who contributed to the Font Software.
>
> PERMISSION & CONDITIONS\
> Permission is hereby granted, free of charge, to any person obtaining
> a copy of the Font Software, to use, study, copy, merge, embed, modify,
> redistribute, and sell modified and unmodified copies of the Font
> Software, subject to the following conditions:
>
> 1) Neither the Font Software nor any of its individual components,
> in Original or Modified Versions, may be sold by itself.
>
> 2) Original or Modified Versions of the Font Software may be bundled,
> redistributed and/or sold with any software, provided that each copy
> contains the above copyright notice and this license. These can be
> included either as stand-alone text files, human-readable headers or
> in the appropriate machine-readable metadata fields within text or
> binary files as long as those fields can be easily viewed by the user.
>
> 3) No Modified Version of the Font Software may use the Reserved Font
> Name(s) unless explicit written permission is granted by the corresponding
> Copyright Holder. This restriction only applies to the primary font name as
> presented to the users.
>
> 4) The name(s) of the Copyright Holder(s) or the Author(s) of the Font
> Software shall not be used to promote, endorse or advertise any
> Modified Version, except to acknowledge the contribution(s) of the
> Copyright Holder(s) and the Author(s) or with their explicit written
> permission.
>
> 5) The Font Software, modified or unmodified, in part or in whole,
> must be distributed entirely under this license, and must not be
> distributed under any other license. The requirement for fonts to
> remain under this license does not apply to any document created
> using the Font Software.
>
> TERMINATION\
> This license becomes null and void if any of the above conditions are
> not met.
>
> DISCLAIMER\
> THE FONT SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
> EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO ANY WARRANTIES OF
> MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT
> OF COPYRIGHT, PATENT, TRADEMARK, OR OTHER RIGHT. IN NO EVENT SHALL THE
> COPYRIGHT HOLDER BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
> INCLUDING ANY GENERAL, SPECIAL, INDIRECT, INCIDENTAL, OR CONSEQUENTIAL
> DAMAGES, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
> FROM, OUT OF THE USE OR INABILITY TO USE THE FONT SOFTWARE OR FROM
> OTHER DEALINGS IN THE FONT SOFTWARE.
</details>

Although the current version is Source Serif 4, Flood uses [an earlier version
of the font](https://fonts.google.com/specimen/Source+Serif+Pro), which went by
the name Source Serif Pro.

### Ubuntu

The [Ubuntu font family](https://design.ubuntu.com/font/) is designed for
Canonical Ltd. by typographers at the font foundry Dalton Maag, with some other
[contributors](https://launchpad.net/~uff-contributors).

> Copyright 2010,2011 Canonical Ltd.
>
> This Font Software is licensed under the Ubuntu Font Licence, Version
1.0.  https://launchpad.net/ubuntu-font-licence

As stated in the copyright notice, [**it is
licensed**](https://ubuntu.com/legal/font-licence) under the Ubuntu Font
License, Version 1.0:

<details>
<summary>
<strong>View Ubuntu font license</strong>&nbsp;&nbsp;&nbsp;
<em>&ldquo;This licence allows the licensed fonts to be used, studied, modified and redistributed&hellip;&rdquo;</em>
</summary>

> # Ubuntu font licence
>
> ## Version 1.0
>
> ### Preamble
>
> This licence allows the licensed fonts to be used, studied, modified and redistributed freely. The fonts, including any derivative works, can be bundled, embedded, and redistributed provided the terms of this licence are met. The fonts and derivatives, however, cannot be released under any other licence. The requirement for fonts to remain under this licence does not require any document created using the fonts or their derivatives to be published under this licence, as long as the primary purpose of the document is not to be a vehicle for the distribution of the fonts.
>
> ### Definitions
>
> “Font Software” refers to the set of files released by the Copyright Holder(s) under this licence and clearly marked as such. This may include source files, build scripts and documentation.
>
> “Original Version” refers to the collection of Font Software components as received under this licence.
>
> “Modified Version” refers to any derivative made by adding to, deleting, or substituting — in part or in whole — any of the components of the Original Version, by changing formats or by porting the Font Software to a new environment.
>
> “Copyright Holder(s)” refers to all individuals and companies who have a copyright ownership of the Font Software.
>
> “Substantially Changed” refers to Modified Versions which can be easily identified as dissimilar to the Font Software by users of the Font Software comparing the Original Version with the Modified Version.
>
> To “Propagate” a work means to do anything with it that, without permission, would make you directly or secondarily liable for infringement under applicable copyright law, except executing it on a computer or modifying a private copy. Propagation includes copying, distribution (with or without modification and with or without charging a redistribution fee), making available to the public, and in some countries other activities as well.
>
> ### Permission & Conditions
>
> This licence does not grant any rights under trademark law and all such rights are reserved.
>
> Permission is hereby granted, free of charge, to any person obtaining a copy of the Font Software, to propagate the Font Software, subject to the below conditions:
>
> 1.  Each copy of the Font Software must contain the above copyright notice and this licence. These can be included either as stand-alone text files, human-readable headers or in the appropriate machine-readable metadata fields within text or binary files as long as those fields can be easily viewed by the user.
> 2.  The font name complies with the following:
>     1.  The Original Version must retain its name, unmodified.
>     2.  Modified Versions which are Substantially Changed must be renamed to avoid use of the name of the Original Version or similar names entirely.
>     3.  Modified Versions which are not Substantially Changed must be renamed to both
>         1.  retain the name of the Original Version and
>         2.  add additional naming elements to distinguish the Modified Version from the Original Version. The name of such Modified Versions must be the name of the Original Version, with “derivative X” where X represents the name of the new work, appended to that name.
> 3.  The name(s) of the Copyright Holder(s) and any contributor to the Font Software shall not be used to promote, endorse or advertise any Modified Version, except
>     1.  as required by this licence,
>     2.  to acknowledge the contribution(s) of the Copyright Holder(s) or
>     3.  with their explicit written permission.
> 4.  The Font Software, modified or unmodified, in part or in whole, must be distributed entirely under this licence, and must not be distributed under any other licence. The requirement for fonts to remain under this licence does not affect any document created using the Font Software, except any version of the Font Software extracted from a document created using the Font Software may only be distributed under this licence.
>
> ### Termination
>
> This licence becomes null and void if any of the above conditions are not met.
>
> ### Disclaimer
>
> THE FONT SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT OF COPYRIGHT, PATENT, TRADEMARK, OR OTHER RIGHT. IN NO EVENT SHALL THE COPYRIGHT HOLDER BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, INCLUDING ANY GENERAL, SPECIAL, INDIRECT, INCIDENTAL, OR CONSEQUENTIAL DAMAGES, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF THE USE OR INABILITY TO USE THE FONT SOFTWARE OR FROM OTHER DEALINGS IN THE FONT SOFTWARE.
</details>
