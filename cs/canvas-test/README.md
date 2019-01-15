# Readme

## Beschreibung

Dies ist, wie der Name sagt, ein `C#`-Projekt zum Testen von Möglichkeiten zum Zeichnen.

## Implementierung

Momentan wird eine manuell erstellte `Bitmap` in eine `PictureBox` eingebunden und dann beschrieben, d.h. die einzelnen Pixel der `Bitmap` werden nacheinander gesetzt.

Dazu wurde eine Klasse `Graph` erstellt, welche die `PictureBox`, die `Bitmap`, sowie sämtliche Funktionen bezüglich dieser kapselt.
Unter diesen Funktionen sind `DrawLine` und `DrawLines`, welche zum Zeichnen von Linien in der `Bitmap` Anhand von Pixel-Koordinaten genutzt werden kann.
*Wichtig* hierbei ist: Pixel-Koordinaten der `Bitmap` haben ihren Ursprung, also die `(0,0)`-Koordinate, Oben-Links.

## Aussicht

Da momentan noch nicht feststeht, auf welche Art und Weise wir die Graphen zeichnen wollen, ist alles in diesem Projekt reine Spielwiese.