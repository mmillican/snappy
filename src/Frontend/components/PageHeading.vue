<template>
  <h1 :class="[{ 'truncate': props.truncate }, 'text-3xl font-bold leading-tight tracking-tight text-gray-900']">
    <slot />
  </h1>
</template>

<script lang="ts" setup>

type Props = {
  truncate?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  truncate: false,
});
</script>
