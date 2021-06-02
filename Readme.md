# GreenFloat
ScreenFloat-like application for Windows, works via Greenshot's "External command Plugin".

## Setup
1. Place "GreenFloat.exe" in a known location
2. Right-click the Greenshot icon in the task tray and select "Preferences"
3. On the "Plugins" tab, select the "External command Plugin" and click "Configure"
4. Click the "New" button
5. Name the command whatever you would like ex: GreenFloat
6. In the "Command" box, use the browse button to locate "GreenFloat.exe"
7. Enter "{0}" in the "Argument" box (including the double-quotes, this is the default)
8. Click "Ok" until the "Settings" window has closed

#### Notes
- On the "Capture" tab of the Greenshot "Settings" window, you may want to uncheck "Show notifications". The "External command Plugin" does not display the "Exported to ..." notification until the external application has exited.
- If you intend to use GreenFloat as the only destination for Greenshot captures, uncheck the "Select destination dynamically" box on the "Destination" tab, and check the box next to GreenFloat.

## Usage
- GreenFloat works like any other Greenshot destination - use Greenshot to capture screens as you normally would, and select "GreenFloat" when prompted to select a destination. The GreenFloat window will be centered on the mouse cursor when launched.
- Clicking and dragging at any point inside the GreenFloat window will move the window.
- Double-clicking anywhere inside of the GreenFloat window will maximize / restore the window. 
- Resizing the window using the corner grip will scale the image to fit, maintaining aspect ratio.
- The mousewheel can be used to zoom in and out on the displayed image at the location of the cursor.
- Right-clicking anywhere on the image displayed by GreenFloat will provide the following menu items. Keyboard shortcuts for these operations are also displayed.
	- **Open** - creates an Open File Dialog that allows you to browse for an image to display.
	- **Save As** - creates a Save File Dialog that allows you to save the currently displayed image to a chosen file path.
	- **Copy** - copies the currently displayed image to the Clipboard.
	- **Reset Zoom** - Resets the mousewheel zoom feature back to the original zoom level and positioning.
	- **Exit** - Exits the application.
- Dragging and dropping a supported image type (.png, .jpg, .jpeg, .bmp) onto the GreenFloat window will load that image in GreenFloat.

