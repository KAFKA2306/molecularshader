# VRChat Molecular Visualization System

A Unity/VRChat molecular visualization system that downloads XYZ molecular data from web sources and renders 3D ball-and-stick models using compute shaders.

## Features

- **Web-based XYZ data loading** using VRChat's String Loading API
- **Real-time raymarched rendering** with compute shaders
- **Ball-and-stick molecular representation** with element-specific colors
- **Interactive UI** with dropdown molecule selection
- **Support for common molecules** (ethanol, water, benzene, and extensible)

## Setup Instructions

### 1. VRChat SDK Setup
1. Install VRChat SDK - Worlds in Unity
2. Install UdonSharp package
3. Add koyashiro's VPM repository: `https://vpm.koyashiro.net/index.json`

### 2. Scene Setup
1. Create a new scene or use existing VRChat world
2. Create an empty GameObject and attach:
   - `MoleculeDownloader` script
   - `MoleculeRaymarchDriver` script
   - `MoleculeUI` script

### 3. UI Setup
1. Create a Canvas with:
   - **TMP_Dropdown** for molecule selection
   - **RawImage** for displaying the rendered molecule
   - **Button** for loading the selected molecule
2. Connect the UI components to the `MoleculeUI` script
3. Connect the `MoleculeUI` to the downloader and driver scripts

### 4. Compute Shader Setup
1. Assign the `MoleculeRaymarch.compute` shader to the `MoleculeRaymarchDriver`
2. Configure rendering settings (resolution, atom scale, lighting)
3. Set up camera transform reference

### 5. URL Configuration
Configure molecule URLs in `MoleculeDownloader`:
```csharp
public string[] keys = {"ethanol", "water", "benzene"};
public string[] urls = {
    "https://your-cdn.com/molecules/ethanol.xyz",
    "https://your-cdn.com/molecules/water.xyz", 
    "https://your-cdn.com/molecules/benzene.xyz"
};
```

## Usage

### Local Testing
1. Set `useLocalTestData = true` in `MoleculeUI`
2. Test data files are included in `TestData/` folder
3. Use the `LoadLocalTestData()` method for testing

### VRChat Deployment
1. Host XYZ files on a CORS-enabled web server
2. Update URLs in `MoleculeDownloader`
3. Set `useLocalTestData = false`
4. Build and upload to VRChat

## File Structure

```
Assets/Molecule/
├── Scripts/
│   ├── MoleculeDownloader.cs      # Web download & XYZ handling
│   ├── MoleculeRaymarchDriver.cs  # Compute shader controller
│   └── MoleculeUI.cs              # UI management
├── Shaders/
│   └── MoleculeRaymarch.compute   # Raymarching renderer
└── TestData/
    ├── ethanol.xyz                # Test molecules
    ├── water.xyz
    └── benzene.xyz
```

## XYZ Format Support

The system supports standard XYZ molecular format:
```
[atom_count]
[comment_line]
[element] [x] [y] [z]
[element] [x] [y] [z]
...
```

Example (water):
```
3
water
O   0.000   0.000   0.000
H   0.757   0.586   0.000
H  -0.757   0.586   0.000
```

## Supported Elements

- H (Hydrogen) - White
- C (Carbon) - Dark Gray  
- N (Nitrogen) - Blue
- O (Oxygen) - Red
- F (Fluorine) - Cyan
- P (Phosphorus) - Orange
- S (Sulfur) - Yellow
- Cl (Chlorine) - Green

## Configuration Options

### MoleculeRaymarchDriver
- `resolution`: Render target resolution
- `atomScale`: Scaling factor for atom spheres
- `bondScale`: Scaling factor for bonds
- `bondDistanceThreshold`: Maximum distance for bond detection
- `lightDirection`: Lighting direction vector

### Performance Notes
- Larger molecules may impact performance
- Consider reducing resolution for complex molecules
- Bond detection is based on distance thresholds
- Compute shader optimized for small to medium molecules

## Extending the System

### Adding New Molecules
1. Add molecule name to `keys` array
2. Add corresponding URL to `urls` array
3. Host XYZ file on web server

### Adding New Elements
1. Update `ElementMap` dictionary
2. Add color and radius to `ElementColors` and `ElementRadii` arrays
3. Recompile and test

## Troubleshooting

- **No molecule visible**: Check camera position and molecule scale
- **Download fails**: Verify URL accessibility and CORS settings
- **Parsing errors**: Ensure XYZ file format is correct
- **Performance issues**: Reduce resolution or atom count

## Dependencies

- Unity 2022.3+ (VRChat compatible)
- VRChat SDK - Worlds
- UdonSharp
- VRChat String Loading API