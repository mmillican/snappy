<template>
  <Head>
    <Title>
      {{ album?.title }}
    </Title>
  </Head>

  <div>
    <NuxtLayout name="app-page">
      <template #title>
        {{ album?.title }}
      </template>

      <ul role="list" class="grid grid-cols-2 gap-x-4 gap-y-8 sm:grid-cols-3 sm:gap-x-6 lg:grid-cols-4 xl:gap-x-8">
        <li v-for="photo in albumPhotos" :key="photo.id" class="relative">
          <div class="group aspect-h-4 aspect-w-6 block w-full overflow-hidden rounded-lg bg-gray-100 focus-within:ring-2 focus-within:ring-indigo-500 focus-within:ring-offset-2 focus-within:ring-offset-gray-100">
            <NuxtLink :to="`/albums/${route.params.slug}/photos/${photo.id}`">
              <img :src="photo.sizes.medium" alt="" class="pointer-events-none object-cover group-hover:opacity-75" />
            </NuxtLink>
          </div>
          <p class="pointer-events-none mt-2 block truncate text-sm font-medium text-gray-900">{{ photo.title }}</p>
          <p class="pointer-events-none block text-sm font-medium text-gray-500">
            {{ friendlyLocalDate(photo.captureDate || photo.createdOn) }}
          </p>
        </li>
      </ul>
    </NuxtLayout>
  </div>
</template>

<script lang="ts" setup>
import { Album, Photo } from '~~/models';
import { friendlyLocalDate } from '~~/utils/dates';

const route = useRoute();

const { data: album } = await useAsyncData<Album>('album', () =>
  $snappyFetch(`albums/${route.params.slug}`)
);

const { data: albumPhotos } = await useAsyncData<Photo[]>('photos', () =>
  $snappyFetch(`photos?albumSlug=${route.params.slug}`)
);

</script>
