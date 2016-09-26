#!/usr/bin/python
# coding: utf8

from myDeezerApp import *
import curses


def control_loop(app):
    pass


def main():
    app = MyDeezerApp(True)
    app.log_info()
    if len(sys.argv) != 2:
        app.argv_error()
        return 1
    app.set_song(sys.argv[1])
    app.start()
    control_loop(app)
    var = raw_input("Enter something: ")
    print "You entered "+var
    return 0


if __name__ == "__main__":
    main()
