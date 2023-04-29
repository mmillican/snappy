<template>
  <!-- Intentionally does not use the `app-page` layout -->
  <div v-if="photo">
    <div class="bg-gray-200 flex justify-center">
      <img :src="photo.sizes.medium" :alt="photo.title" />
    </div>

    <div class="mx-auto max-w-7xl sm:px-6 lg:px-8 mt-4 grid sm:grid-cols-12">
      <div class="col-span-8">
        <PageHeading truncate>
          {{ photo.title }}
        </PageHeading>

      </div>
      <div class="col-span-4">
        <h2 v-if="photo.captureDate" class="text-gray-500">
          Taken {{ friendlyLocalDate(photo.captureDate) }}
        </h2>
        <h2 class="text-gray-500">
          Uploaded {{ friendlyLocalDate(photo.createdOn) }}
        </h2>
      </div>


    </div>
  </div>
</template>

<script lang="ts" setup>
import { Photo } from '~~/models';
import { friendlyLocalDate } from '~~/utils/dates';

const route = useRoute();

const { data: photo } = await useAsyncData<Photo>('photo',
  () => $snappyFetch(`photos/${route.params.photoId}?albumSlug=${route.params.slug}`)
);

console.log('data', photo.value);
</script>
