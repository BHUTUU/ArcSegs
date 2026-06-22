# ArcSegs

Convert any curve into a lightweight polyline made of straight segments.

`ArcSegs` recreates the selected object as a new polyline by sampling points along its length and then erasing the original entity.

## Features

* Supports:

  * Arc
  * Circle
  * Spline

* Two commands:

  * **ARCSEGS** → Specify the total number of segments.
  * **ARCSEGS2** → Creates **2 segments per unit length**.

* Preserves:

  * Layer
  * Color
  * Linetype
  * Lineweight
  * Linetype Scale

## Usage

### ARCSEGS

```text
Command: ARCSEGS
Enter total number of segments: 20
Select objects:
```

Creates a new polyline with the specified number of equal segments.

### ARCSEGS2

```text
Command: ARCSEGS2
Select objects:
```

Creates a new polyline with **2 segments per unit length**.

## Requirements

* AutoCAD 2025
* .NET 8
* x64 Platform

## Installation

1. Build the project in **Release | x64**.
2. Open AutoCAD.
3. Run:

```text
NETLOAD
```

4. Select the generated DLL.
5. Run `ARCSEGS` or `ARCSEGS2`.

## Author

**Suman Kumar**

GitHub: https://github.com/BHUTUU
