export default function deepFreeze<T>(obj: T): T {
  const propNames = Object.getOwnPropertyNames(obj)
  for (const name of propNames) {
    const value = (obj as { [key: string]: any })[name]

    if (value && typeof value === 'object') {
      deepFreeze(value)
    }
  }
  return Object.freeze(obj)
}
