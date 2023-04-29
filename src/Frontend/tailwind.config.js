const defaultTheme = require('tailwindcss/defaultTheme');

module.exports = {
	theme: {
		extend: {
			fontFamily: {
				sans: ["Inter", ...defaultTheme.fontFamily.sans],
			},
		},
	},
	variants: {},
	plugins: [
		// require("@tailwindcss/ui")
    require('@tailwindcss/aspect-ratio'),
    require('@tailwindcss/forms'),
	]
}
