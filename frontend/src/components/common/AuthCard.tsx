import type { ReactNode } from 'react';

interface AuthCardProps {
  icon?: string;
  title: string;
  subtitle?: string;
  children: ReactNode;
  footer?: ReactNode;
}

export function AuthCard({ icon, title, subtitle, children, footer }: AuthCardProps) {
  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="w-full max-w-md p-8 bg-white rounded-xl shadow-sm border border-gray-100">
        <div className="mb-8 text-center">
          {icon && <div className="text-4xl mb-3">{icon}</div>}
          <h1 className="text-2xl font-bold text-gray-900">{title}</h1>
          {subtitle && (
            <p className="text-gray-500 text-sm mt-1">{subtitle}</p>
          )}
        </div>

        {children}

        {footer && (
          <div className="mt-6 text-center text-sm">
            {footer}
          </div>
        )}
      </div>
    </div>
  );
}
