module Text
    open SharpFont

    let private sharpFontLib = new SharpFont.Library()

    let getFont path = new Face(sharpFontLib, path)

    