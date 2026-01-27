import { FallbackProps } from 'react-error-boundary';

export default function ErrorFallback({ error, resetErrorBoundary }: FallbackProps) {
  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-gray-950 px-4">
      <div className="max-w-lg w-full glass-card p-6">
        <h1 className="text-xl font-semibold text-gray-900 dark:text-gray-100">
          Something went wrong
        </h1>
        <p className="mt-2 text-sm text-gray-700 dark:text-gray-300">
          The app hit an unexpected error. You can try reloading the UI.
        </p>

        <pre className="mt-4 p-3 bg-gray-100 dark:bg-gray-800 rounded text-xs text-gray-800 dark:text-gray-200 overflow-auto">
          {error?.message ?? String(error)}
        </pre>

        <div className="mt-4 flex gap-2">
          <button
            onClick={resetErrorBoundary}
            className="inline-flex justify-center rounded-md bg-primary-600 px-4 py-2 text-sm font-medium text-white hover:bg-primary-700 transition-colors"
          >
            Try again
          </button>
          <button
            onClick={() => window.location.reload()}
            className="inline-flex justify-center rounded-md glass-card px-4 py-2 text-sm font-medium text-gray-900 dark:text-gray-100 hover:bg-white/90 dark:hover:bg-gray-800/90 transition-colors"
          >
            Reload page
          </button>
        </div>
      </div>
    </div>
  );
}
