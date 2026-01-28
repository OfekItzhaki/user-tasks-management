import { getVersionString, getBuildInfoString } from '../utils/version';

/**
 * Footer component displaying application version and build information
 */
export default function VersionFooter() {
  return (
    <footer className="mt-8 py-4 text-center text-sm text-gray-500 dark:text-gray-400">
      <div className="flex flex-col items-center gap-1">
        <span>{getVersionString()}</span>
        <span className="text-xs opacity-75">{getBuildInfoString()}</span>
      </div>
    </footer>
  );
}
