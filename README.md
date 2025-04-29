# ag-shared-ui-elements

Awesome Golf Shared UI Elements

This repo exists so we don’t have to copy-paste, restyle, or rewire the same UI over and over. Due to the current state of UI Toolkit, as of 29/04/2025 we have several limitations. These are listed below.

## How to Use the UI Elements

### 1. Add a Shared UI Element to Your Scene

```csharp
var buttonScene = (PackedScene)ResourceLoader.Load("res://ui/shared/button_template.tscn");
var buttonContainer = buttonScene.Instance();
AddChild(buttonContainer);
```

### 2. Access a Child Node Safely

Since child names are fixed, you’ll need to access them like this:

```csharp
var label = buttonContainer.GetNode("MarginContainer/Label");
label.Text = "Submit";
```

### 3. Don’t Touch the Folder Structure

Keep everything where it is. Moving files or folders will break style references and cause visual issues.

---

## Limitations to Keep in Mind ⚠️

This repo contains a shared set of UI elements built with a UI toolkit. They’re super handy, but there are a few important things to know before using or modifying them:

### Template Containers

All elements are wrapped in **Template containers**. These are a bit restrictive:
- You **can’t rename** child nodes inside them.
- Because of that, you’ll need to **access elements in two steps**:
  1. Grab the top-level container.
  2. Then get the child element you need from there.

### Double Node Lookup

You won’t be able to directly access nested nodes. Instead, do something like this:

```csharp
var container = GetNode("MyButtonContainer");
var button = container.GetNode("MarginContainer/Button");
button.Text = "Click Me";
```

Yes, it’s a bit more verbose — but it keeps everything consistent and reusable.

### Styles Use Relative Paths

The UI styling is handled using **USS files** with **relative paths**.  
This means:
- **Don’t move folders or files around.**
- Changing the folder structure will **break all the UI styles** — really, all of them.

For example, styles are loaded like this:
```uss
@import "../styles/button.uss";
```
If you move anything, paths like that will stop working.

---
