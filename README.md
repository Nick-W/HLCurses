HLCurses
========

High-Level &quot;Curses&quot; implementation in pure C#

What is it?
Not really curses, but rather an element based window-ish system for the console.

What does it do?
Draw boxes, organize them into grids, embed objects in other objects.

What does it not do?
Everything else.  Literally.  This was written in one day - don't expect much just yet.

Notes
=====
1. drawBlock uses native CLR calls when writing to the console, which means it is beyond slow.
   This is an easy fix down the road for those who need some form of interactivity.


If you use this, I'm certain you'll be doing some (tons of) hacking on the code, so please make a pull request if you do!