# Interface Accessibility Analyzer

A desktop tool for automated accessibility analysis of user-interface screenshots, built in Unity as part of a master's thesis. You load one or more screenshots (PNG/JPG), and the tool runs a computer-vision and OCR pipeline over each one. It then computes an **accessibility scale score** and generates a detailed **PDF report** for every screen.

The analysis focuses on two groups of users:

- **people with visual impairments**: color and contrast issues
- **autistic people**: text readability issues (font size, line spacing, alignment, busy backgrounds)

Most thresholds follow **WCAG 2.x (level AA)** guidelines. Where no guideline exists, the values were chosen experimentally.

---

## How it works

Each screenshot passes through a four-stage pipeline (`Assets/Scripts/Analysis`), orchestrated by `ImageProcessingManager`:

```
Screenshot ──► 1. Preprocessing ──► 2. Properties ──► 3. Accessibility ──► 4. Report (PDF)
```

### 1. Preprocessing (`PreprocessingStage`)
- **Dominant colors**: k-means clustering (HSV, Gaussian-blurred) finds the 5 colors that cover most of the interface.
- **Color segmentation**: the image is quantized to its k dominant colors.
- **Edge and contour detection**: Canny edge detection plus contour extraction (OpenCV).
- **OCR**: Tesseract recognizes words and their bounding boxes. The words are then grouped into a document structure: *words → lines → text blocks*.
- **Region segmentation**: a recursive divide-and-conquer split of the image into rectangles. Splitting stops when a region holds roughly ≤ 3 perceptually distinct colors (distance in CIE Lab), has low histogram entropy, or is smaller than 10×10 px. Segments that overlap detected text can optionally be excluded.

### 2. Properties processing (`PropertiesProcessingStage`)
- Geometric properties of words, lines, text blocks and the whole document (size, area, spacing, alignment).
- Text and background color for each word, and whether the background is uniform or image-like (entropy and edge density).
- **WCAG contrast ratio** (relative luminance formula) for each segment and for each pair of dominant colors.

### 3. Accessibility processing (`AccessibilityProcessingStage`)
Each feature becomes an **indicator** whose base value is usually the percentage of the screen area it affects. Every indicator is multiplied by a configurable **weight**:

| Group | Indicator | Criterion |
|-------|-----------|-----------|
| Color | Dominant colors with low contrast | contrast ratio < 4.5:1 |
| Color | Dominant colors with high contrast | contrast ratio > 15:1 |
| Color | Segments with low contrast | contrast ratio < 4.5:1 |
| Color | Segments with high contrast | contrast ratio > 15:1 |
| Text | Low text/background contrast | contrast ratio < 4.5:1 |
| Text | High text/background contrast | contrast ratio > 15:1 |
| Text | Image used as text background | background entropy higher than the text's |
| Text | Font too small | line height < 14 px |
| Text | Line spacing too small | interline < 1.5 × font height |
| Text | Right-aligned text | right edges of lines/blocks aligned |

```
Color score   = Σ (color indicator × weight)
Text score    = Σ (text indicator × weight)
Overall score = Color score + Text score
```

**Lower is better.** The aim is to minimize the accessibility scale value.

### 4. Report generation (`ReportGenerationStage`)
A PDF report (iTextSharp, Roboto font) is generated for each screen. It contains:
- the analyzed screenshot and the overall, text and color scores
- a table with each indicator's value and weight
- the dominant color palette and the color-segmented image
- the segmentation map and a contrast visualization (red: low contrast, blue: high contrast)
- the OCR result with the detected words highlighted
- a breakdown of each text indicator with a visualization, plus notes on known limitations

Reports are saved to:

```
<Documents>/AccessibilityAnalysisRaports/AccessibilityAnalysisRaport_<screen>_<timestamp>_<date>.pdf
```

> **Note:** the generated reports are currently written in **Polish**.

---

## Using the application

The app has two scenes:

1. **Config scene** (`Assets/Scenes/ConfigScene.unity`, the start scene)
   - **Upload** screenshots (`.png`, `.jpg`, `.jpeg`). They are stored in `Application.persistentDataPath/Screens` and stay available between sessions.
   - **Select** the screens you want to analyze with their checkboxes, or remove ones you no longer need.
   - **Settings**: enable or disable each indicator and set its weight (0.01–1.0). The settings are saved to `Application.persistentDataPath/analysis_parameters.json`.
   - **Analyse**: runs the pipeline on every selected screen and shows progress in the log panel.
2. **Image processing scene** (`Assets/Scenes/ImageProcessingScene.unity`)
   - Browse the results (previous/next screen).
   - Open the generated PDF, or the folder that contains the reports.

---

## Getting started

### Requirements
- **Unity 2022.3.15f1** (LTS)
- Windows x64 (the main target; Tesseract binaries for macOS and Android are included but untested)

### Setup
1. Clone the repository:
   ```bash
   git clone https://github.com/swrczk/MGR.git
   ```
2. Open the project in Unity Hub with Unity 2022.3.15f1. The Package Manager fetches UniTask automatically from Git, so Git must be installed and on your `PATH`.
3. Open `Assets/Scenes/ConfigScene.unity` and press **Play**, or build a standalone player via *File → Build Settings* (both scenes are already in the build list).

Tesseract language data (`eng`) ships in `Assets/StreamingAssets/tessdata`.

### Tuning image-processing parameters
Low-level thresholds live in the `ImageProcessingParametersSo` asset (`Assets/Settings/ImageProcessing/DefaultImageProcessingParameters.asset`):

| Parameter | Default | Meaning |
|-----------|---------|---------|
| `clusterCount` | 5 | number of dominant colors (k in k-means) |
| `excludeWords` | true | exclude text regions from segment contrast analysis |
| `targetColorAmount` | 3 | max distinct colors per segment |
| `colorThreshold` | 50 | Lab distance for two colors to count as distinct |
| `entropyThreshold` | 0.3 | entropy below which a region stops splitting |
| `contrastLowerThreshold` | 4.5 | low-contrast threshold (WCAG AA) |
| `contrastUpperThreshold` | 15.0 | high-contrast threshold (experimental) |
| `entropyThresholdWords` | 5.0 | entropy threshold for an image-like text background |
| `edgesPerCharThreshold` | 10 | edge density threshold for word backgrounds |

Indicator weight presets are in `Assets/Settings/AnalysisWeights/`.

---

## Project structure

```
Assets/
├── Scenes/                     ConfigScene, ImageProcessingScene (+ sample/test scenes)
├── Scripts/
│   ├── Analysis/
│   │   ├── PreprocessingStage/          color clustering, OCR, segmentation
│   │   ├── PropertiesProcessingStage/   text geometry, colors, contrast
│   │   ├── AccessibilityProcessingStage/ indicators and scores
│   │   ├── ReportGenerationStage/       PDF generation and visualizations
│   │   └── SOs/                         ScriptableObject parameters and models
│   ├── Manager/                managers (upload, results, settings, processing)
│   ├── UI/                     MVC views/controllers for both scenes
│   ├── Tesseract/              Tesseract wrapper and driver
│   └── Utils/                  MVC base classes, singleton, helpers
├── Settings/                   parameter assets
├── Plugins/                    OpenCV+Unity, Tesseract, iTextSharp
├── StandaloneFileBrowser/      native file dialogs
└── StreamingAssets/            tessdata, fonts
```

---

## Known limitations

- **OCR quality varies.** Tesseract may miss text (aim for at least 50% coverage), misread words, or detect characters where there are none. Results can differ between runs, so re-running the analysis can help. Text indicators are skipped when fewer than 20 words are detected.
- **Contrast of dominant colors** compares every pair in the palette, even colors that never appear next to each other.
- **Segments are analyzed in isolation**, without context. Gradients and 3D objects segment poorly.
- **Image-as-background detection** is unreliable for decorative or highly detailed fonts.
- **Right-alignment detection** gives false positives for menus and other single-item lines.
- Very legible black-on-white text can exceed the high-contrast threshold.

---

## Tech stack

- [Unity 2022.3](https://unity.com/) / C#
- [OpenCV for Unity (OpenCvSharp)](https://github.com/shimat/opencvsharp): image processing
- [Tesseract OCR](https://github.com/tesseract-ocr/tesseract): text recognition
- [iTextSharp](https://github.com/itext/itextsharp): PDF generation
- [UniTask](https://github.com/Cysharp/UniTask): async/await
- [UnityStandaloneFileBrowser](https://github.com/gkngkc/UnityStandaloneFileBrowser): native file dialogs
- TextMesh Pro

## License

Released under the [MIT License](LICENSE). © 2024 swrczk
