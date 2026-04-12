declare module 'react-hot-toast' {
  import * as React from 'react';

  export type ToastPosition = 'top-left' | 'top-center' | 'top-right' | 'bottom-left' | 'bottom-center' | 'bottom-right';

  export interface ToastOptions {
    duration?: number;
    position?: ToastPosition;
    icon?: React.ReactNode | { [key: string]: any };
    style?: React.CSSProperties;
    className?: string;
    [key: string]: any;
  }

  export interface ToastPromiseParams {
    loading: React.ReactNode;
    success: React.ReactNode;
    error: React.ReactNode;
  }

  export interface Toast {
    (message: React.ReactNode, opts?: ToastOptions): string;
    success(message: React.ReactNode, opts?: ToastOptions): string;
    error(message: React.ReactNode, opts?: ToastOptions): string;
    promise<T>(p: Promise<T>, msgs: ToastPromiseParams, opts?: ToastOptions): Promise<T>;
    dismiss(id?: string | number): void;
  }

  export const Toaster: React.FC<{ position?: ToastPosition; toastOptions?: any }>;
  const toast: Toast;
  export default toast;
}
