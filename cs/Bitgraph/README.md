# Readme

## Beschreibung

Dies ist das `C#`-Programm zur Abgabe für das Projekt.
Es ist eine Windows 64-Bit Anwendung und wurde unter Nutzung des *.NET Framework* der Version `4.6.1` erstellt.

## Implementierung

### Visualisierung des Graphen

Es wird eine manuell erstellte `Bitmap` (i.F. `DrawArea`) in eine `PictureBox` (i.F. `DrawAreaContainer`) eingebunden und dann beschrieben, d.h. die einzelnen Pixel der `Bitmap` werden nacheinander gesetzt.

Dazu gibt es eine Klasse `GraphManager`, welche den `DrawAreaContainer`, die `DrawArea`, sowie sämtliche Funktionen bezüglich dieser kapselt.
Unter diesen Funktionen ist `DrawLine`, welche zum Zeichnen von Linien in der `DrawArea` Anhand von Pixel-Koordinaten genutzt werden kann.
*Wichtig* hierbei ist: Es existiert ein gravierender Unterschied zwischen visuellen und geometrischen Koordinaten.

Visuelle Koordinaten der `DrawArea` sind vom Typ `int` und haben ihren Ursprung, also die `(0,0)`-Koordinate, in der oberen linken Ecke.
Ihr Wertebereich ist begrenzt auf `0 <= x < Breite` bzw. `0 <= y < Höhe` und ein Versuch auf Pixel ausserhalb dieser Grenzen zuzugreifen führt zu einem Fehler.
Die Höhe und Breite der `DrawArea` ist abhängig vom `DrawAreaContainer`, welcher sich wiederum an die Größe des überliegenden Objektes anpasst.

Geometrische Koordinaten des `Graph` sind vom Typ `float` und haben ihren gewohnten geometrischen Koordinatenursprung.
Der Wertebereich des Graphen ist beschränkt auf die Werte, welche in `float` abbildbar sind.

Es existiert eine `private`-Hilfsfunktion zum Umrechnen von geometrischen Koordinaten des Graphen in visuelle Koordinaten der Bitmp.

Die darzustellenden polynomiellen Funktionen werden nun mit Hilfe ihrer Koeffizienten im Code repräsentiert und gezeichnet.
Andere Methoden, etwa eine Annäherung einer Kurve an die gegebenen Funktionspunkte, wurden zwar getestet, haben aber für ihren benötigten Aufwand nicht zu ausreichenden Ergebnissen geführt.

## Aussicht

Da momentan noch nicht feststeht, auf welche Art und Weise wir die Graphen zeichnen wollen, ist alles in diesem Projekt reine Spielwiese.

**Update:** Nach mehreren Besprechungen und einigem Testen ist das grundlegende Layout des Programms entstanden und die momentane Vorgehensweise beim Zeichnen des Graphen, sowie beim Festhalten/ Eintragen der Werte hat sich gebildet.