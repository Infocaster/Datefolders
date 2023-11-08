export function ensureExists<T>(obj: T | undefined, msg: string): asserts obj is T {

    if (!obj) throw new Error(msg);
}

export function ensureServiceExists<T>(obj: T | undefined, name: string): asserts obj is T {

    ensureExists(obj, `expected '${name}' to exist, but it doesn't`);
}