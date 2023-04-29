// https://nuxt.com/docs/api/configuration/nuxt-config
export default {
	typescript: {
		shim: false,
	},
	modules: [
    // Doc: https://github.com/nuxt-community/nuxt-tailwindcss
    "@nuxtjs/tailwindcss",
    // Doc: https://github.com/nuxt-community/google-fonts-module
    "@nuxtjs/google-fonts"
  ],
	googleFonts: {
		families: {
			Inter: true,
		},
	},
	tailwindcss: {
		jit: true,
	},
  runtimeConfig: {
    public: {
      apiBaseUrl: process.env.API_BASE_URL,
    },
  },
  imports: {
    dirs: ['./utils'],
  },
};
