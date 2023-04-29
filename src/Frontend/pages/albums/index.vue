<template>
  <div>
    <NuxtLayout name="app-page">

      <template #title>
        Albums
      </template>

      <div class="grid grid-cols-1 gap-y-4 sm:grid-cols-2 sm:gap-x-6 sm:gap-y-10 lg:grid-cols-4 lg:gap-x-8">
        <div
          v-for="album in albums"
          :key="album.slug"
          class="group relative flex flex-col overflow-hidden rounded-lg border border-gray-200"
        >
        <div class="aspect-w-3 aspect-h-4 bg-gray-200 group-hover:opacity-75 sm:aspect-none sm:h-96">
          <img :src="'https://tailwindui.com/img/ecommerce-images/category-page-02-image-card-01.jpg'" :alt="album.title" class="h-full w-full object-cover object-center sm:h-full sm:w-full" />
        </div>
        <div class="flex flex-1 flex-col space-y-2 p-4">
          <h3 class="text-sm font-medium text-gray-900">
            <NuxtLink :to="`/albums/${album.slug}`">
              <span aria-hidden="true" class="absolute inset-0" />
              {{ album.title }}
            </NuxtLink>
          </h3>
          <p class="text-sm text-gray-500">{{ album.description || album.title }}</p>
          <div class="flex flex-1 flex-col justify-end">
            <p class="text-sm italic text-gray-500">{{ album.createdOn }}</p>
          </div>
        </div>
        </div>
      </div>
    </NuxtLayout>

  </div>

</template>

<script lang="ts" setup>
import { Album } from '~~/models/album';

definePageMeta({
  title: 'Albums',
  layout: false,
  // layout: 'app-page',
})
useHead({
  title: 'Albums',
})

const { data: albums } = await useAsyncData<Album[]>(() => $snappyFetch(`albums`));
</script>
