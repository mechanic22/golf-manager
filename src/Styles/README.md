# Styles

This folder contains the Tailwind CSS configuration and source files for the GolfManager Blazor WebAssembly application.

## Structure

```
src/Styles/
├── tailwind.config.js      # Tailwind configuration (theme, colors, animations)
├── postcss.config.js        # PostCSS configuration
└── app-tailwind.css         # Source CSS file (compiled to wwwroot/css/app.css)
```

## Tailwind Configuration

The `tailwind.config.js` file is configured to scan all Razor components in the `GolfManager.Web` project:

- **Content paths:** `./src/GolfManager.Web/**/*.{razor,html,cshtml}`
- **Theme:** Golf-themed color palette (greens, blues)
- **Custom animations:** fade-in, slide-up, bounce-subtle

## CSS Output

The Tailwind CLI compiles the source CSS file into:
- **Output:** `src/GolfManager.Web/wwwroot/css/app.css` (minified)

## Development Workflow

### Watch Mode (Recommended for Development)

Run this command to automatically rebuild CSS when you make changes:

```bash
npm run watch:css
```

This will watch for changes in:
- `src/Styles/app-tailwind.css`
- All Razor components in `src/GolfManager.Web/**/*.razor`

### Build Mode (For Production)

To build the CSS once without watching:

```bash
npm run build:css
```

## Adding Custom Styles

### Option 1: Use Tailwind Utility Classes (Recommended)

The best approach is to use Tailwind's utility classes directly in your Razor components:

```razor
<div class="bg-primary-600 text-white p-4 rounded-lg">
    Hello World
</div>
```

### Option 2: Extend Tailwind Theme

Add custom colors, spacing, or other theme extensions in `tailwind.config.js`:

```javascript
theme: {
  extend: {
    colors: {
      'custom': '#1a73e8',
    },
  },
}
```

### Option 3: Add Custom CSS Classes

For component-specific styles that can't be achieved with utilities, add them to `app-tailwind.css`:

```css
.my-custom-component {
    background-color: var(--color-primary-600);
    color: white;
    padding: 1rem;
    border-radius: 0.5rem;
}
```

## Golf-Themed Color Palette

The following custom colors are available:

- **primary-*** - Main green colors (50-900)
- **fairway-*** - Golf course green (50-900)
- **sky-*** - Sky blue colors (50-900)

Usage example:
```razor
<button class="bg-primary-600 hover:bg-primary-700 text-white">
    Click Me
</button>
```

## Custom Components

The following custom CSS classes are available:

### Buttons
- `.btn` - Base button styles
- `.btn-primary` - Primary green button
- `.btn-secondary` - Secondary gray button
- `.btn-danger` - Danger red button

### Forms
- `.form-input` - Styled input field
- `.form-label` - Form label
- `.form-error` - Error message

### Cards
- `.card` - Basic card with shadow
- `.card-hover` - Card with hover effect

## Tailwind v4 Notes

This project uses **Tailwind CSS v4**, which has some differences from v3:

1. **@theme directive** - Custom theme values are defined using `@theme { }` instead of `tailwind.config.js` theme extension
2. **CSS variables** - Custom colors use CSS variables (e.g., `var(--color-primary-600)`)
3. **@import "tailwindcss"** - Single import instead of separate base/components/utilities

## First Time Setup

If you're setting up the project for the first time:

1. **Install dependencies:**
   ```bash
   npm install
   ```

2. **Build CSS:**
   ```bash
   npm run build:css
   ```

3. **Run the app:**
   ```bash
   dotnet run --project src/GolfManager.Web/GolfManager.Web.csproj
   ```

## Troubleshooting

### CSS not updating?
- Make sure `npm run watch:css` is running
- Check that your Razor files are in the `src/GolfManager.Web` directory
- Verify the output file exists at `src/GolfManager.Web/wwwroot/css/app.css`

### Build errors?
- Ensure Node.js is installed (v18+ recommended)
- Run `npm install` to install dependencies
- Check that all paths in `tailwind.config.js` are correct

