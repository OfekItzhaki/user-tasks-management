interface PaginationProps {
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
  itemsPerPage: number;
  totalItems: number;
  onItemsPerPageChange?: (itemsPerPage: number) => void;
  isRtl?: boolean;
}

export default function Pagination({
  currentPage,
  totalPages,
  onPageChange,
  itemsPerPage,
  totalItems,
  onItemsPerPageChange,
  isRtl = false,
}: PaginationProps) {
  // Validate inputs
  const safePage = Math.max(1, currentPage);
  const safePageSize = Math.max(1, itemsPerPage);
  const safeTotalPages = Math.max(0, totalPages);
  
  const startItem = safeTotalPages > 0 ? (safePage - 1) * safePageSize + 1 : 0;
  const endItem = safeTotalPages > 0 ? Math.min(safePage * safePageSize, totalItems) : 0;

  const getPageNumbers = () => {
    const pages: (number | string)[] = [];
    const maxVisible = 7;

    if (totalPages <= maxVisible) {
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i);
      }
    } else {
      pages.push(1);

      if (currentPage > 3) {
        pages.push('...');
      }

      const start = Math.max(2, currentPage - 1);
      const end = Math.min(totalPages - 1, currentPage + 1);

      for (let i = start; i <= end; i++) {
        pages.push(i);
      }

      if (currentPage < totalPages - 2) {
        pages.push('...');
      }

      pages.push(totalPages);
    }

    return pages;
  };

  return (
    <div className={`flex flex-col sm:flex-row items-center ${isRtl ? 'flex-row-reverse' : ''} justify-between gap-4 mt-6`}>
      <div className={`flex items-center ${isRtl ? 'flex-row-reverse space-x-reverse space-x-2' : 'gap-2'} text-sm text-gray-700 dark:text-gray-300`}>
        <span>
          Showing {startItem} to {endItem} of {totalItems} items
        </span>
        {onItemsPerPageChange && (
          <select
            value={itemsPerPage}
            onChange={(e) => onItemsPerPageChange(Number(e.target.value))}
            className="ml-2 glass-card rounded-xl border-0 px-3 py-1.5 text-sm text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-gray-400 dark:focus:ring-primary-500/50 transition-all duration-200 cursor-pointer"
          >
            <option value={10}>10 per page</option>
            <option value={25}>25 per page</option>
            <option value={50}>50 per page</option>
            <option value={100}>100 per page</option>
          </select>
        )}
      </div>

      {totalPages > 1 && (
        <div className={`flex items-center ${isRtl ? 'flex-row-reverse space-x-reverse space-x-2' : 'gap-2'}`}>
          <button
            type="button"
            onClick={() => onPageChange(currentPage - 1)}
            disabled={currentPage === 1}
            className="px-4 py-2 text-sm font-semibold text-gray-700 dark:text-gray-300 glass-card rounded-xl hover:bg-gray-50 dark:hover:bg-gray-800/80 disabled:opacity-50 disabled:cursor-not-allowed transition-all duration-200"
          >
            Previous
          </button>

          <div className="flex items-center gap-1">
            {getPageNumbers().map((page, index) => {
              if (page === '...') {
                return (
                  <span
                    key={`ellipsis-${index}`}
                    className="px-3 py-2 text-sm font-medium text-gray-700 dark:text-gray-300"
                  >
                    ...
                  </span>
                );
              }

              const pageNum = page as number;
              return (
                <button
                  key={pageNum}
                  type="button"
                  onClick={() => onPageChange(pageNum)}
                  className={`px-4 py-2 text-sm font-semibold rounded-xl transition-all duration-200 ${
                    currentPage === pageNum
                      ? 'bg-gray-700 dark:bg-gradient-to-r dark:from-primary-600 dark:to-purple-600 text-white shadow-lg shadow-gray-500/20 dark:shadow-primary-500/30'
                      : 'text-gray-700 dark:text-gray-300 glass-card hover:bg-gray-50 dark:hover:bg-gray-800/80'
                  }`}
                >
                  {pageNum}
                </button>
              );
            })}
          </div>

          <button
            type="button"
            onClick={() => onPageChange(currentPage + 1)}
            disabled={currentPage === totalPages}
            className="px-4 py-2 text-sm font-semibold text-gray-700 dark:text-gray-300 glass-card rounded-xl hover:bg-gray-50 dark:hover:bg-gray-800/80 disabled:opacity-50 disabled:cursor-not-allowed transition-all duration-200"
          >
            Next
          </button>
        </div>
      )}
    </div>
  );
}
