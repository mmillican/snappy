<template>
  <div class="py-10">
    <header>
      <div class="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <PageHeading v-if="$slots.title">
          <slot name="title" v-if="$slots.title" />
        </PageHeading>
      </div>
    </header>
    <main>
      <div class="mx-auto max-w-7xl sm:px-6 lg:px-8 pt-6">
        <slot />
      </div>
    </main>
  </div>
</template>
