# Interactive development (command line):

In terminal #1, run:

> `lein figwheel main`

In terminal #2, run:

> `./node_modules/electron/dist/Electron.app/Contents/MacOS/Electron .`

In terminal #3, run:

> touch src/dungeonstrike/ui/core.cljs
> lein repl :connect 7888

Once connected, run:

> (figwheel-sidecar.repl-api/cljs-repl "main")

After this starts, you have a REPL connection to the main
process. You can switch to the UI process by doing:

:cljs/quit
(cljs-repl "ui")

If you start getting namespace reference errors like "Cannot read property
'core' of undefined", invoke (reset-autobuild) from the REPL.

REPL functions can be accessed from any namespace as follows:

> (cljs.repl/doc +)

# Interactive development (Cider):

Start figwheel and electron in separate shells as above. Invoke

> touch src/dungeonstrike/ui/core.cljs

to make figwheel wake up. In Emacs, invoke "cider-connect" and specify
localhost, port 7888. Once connected, run:

> (figwheel-sidecar.repl-api/cljs-repl "main")
