#!/usr/bin/python
# coding: utf8

from myDeezerApp import *


def main():
    app = MyDeezerApp(True)
    app.set_song("dzmedia:///track/85509044")
    app.start()
    return 0


if __name__ == "__main__":
    main()
