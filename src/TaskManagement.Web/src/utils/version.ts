/**
 * Application version information
 * These values are injected at build time by Vite
 */

declare const __APP_VERSION__: string;
declare const __BUILD_TIME__: string;

export const APP_VERSION = typeof __APP_VERSION__ !== 'undefined' ? __APP_VERSION__ : 'dev';
export const BUILD_TIME = typeof __BUILD_TIME__ !== 'undefined' ? __BUILD_TIME__ : new Date().toISOString();

/**
 * Get formatted version string
 */
export function getVersionString(): string {
  return `v${APP_VERSION}`;
}

/**
 * Get formatted build info string
 */
export function getBuildInfoString(): string {
  try {
    const buildDate = new Date(BUILD_TIME);
    return `Built: ${buildDate.toLocaleDateString()} ${buildDate.toLocaleTimeString()}`;
  } catch {
    return `Built: ${BUILD_TIME}`;
  }
}

/**
 * Get full version info object
 */
export function getVersionInfo() {
  return {
    version: APP_VERSION,
    buildTime: BUILD_TIME,
    versionString: getVersionString(),
    buildInfoString: getBuildInfoString(),
  };
}
