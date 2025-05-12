# ag-shared-ui-elements

**Revisited**: 12 April 2025 – _Awkward limitations in UI Builder authoring. See below._

This repo exists so we don’t have to copy-paste, restyle, or rewire the same UI over and over.


## Cloning the Repository

To clone this repository **with all nested submodules**, use:

```bash
git clone --recurse-submodules https://github.com/great-detail/ag-shared-ui-elements.git
```

If you've already cloned the repository without the submodules, run the following to fetch them afterward:

```bash
git submodule update --init --recursive
```

## How to Use the UI Elements

### 1. Add a Shared UI Element to Your Scene

Can be done via UI Builder or c# instantiation.

### 2. Style them

Style them by adding USS selectors. You can override EVERY USS property in the children. Alternatively, create concrete classes from the abstract classes e.g. selectors

### 3. Don’t Touch the Folder Structure

Keep everything where it is. Moving files or folders will break style references (see below).

---

## Limitations to Keep in Mind ⚠️

This repo contains a shared set of UI elements built with a UI toolkit, but there are a few important things to know before using or modifying them:

### Template Containers

All UXML elements are wrapped in **Template containers**. These are a bit restrictive:
- You **can’t rename** child elements inside them.
- Because of that, you’ll need to **access elements in two steps**:
  1. Grab the top-level container.
  2. Then get the child element you need from there.
 
**Note:** This breaks custom elements such as SettingsOptions as the custom control can't directly reference the child name (as it can't be set to a unique value). It's recommended to NOT use templates in this case and either unpack the similar template locally, not globally or instantiate WITHOUT the template container.

![image](https://github.com/user-attachments/assets/7b8af7e6-39e0-461a-8561-aae752b5cac3)


### Double Element Lookup

You won’t be able to directly access nested elements. Instead, do something like this:

```csharp
mLeftButtonContainer = this.Q<VisualElement>(cLeftButtonContainerID); // Does not need to be generic, template containers IDs CAN be changed
mLeftButton = mLeftButtonContainer.Q<VisualElement>(cLeftButtonID); // Where the ID is generic
```

Yes, it’s a bit more verbose — but it keeps everything consistent and reusable.

### Styles Use Relative Paths

The UI styling is handled using **USS files** with **relative or absolute paths**.  
This means:
- **Don’t move folders or files around.**
- Changing the folder structure will **break all the UI styles** and require MANUAL editing of USS files (At least in the current version of UI Toolkit)
---
