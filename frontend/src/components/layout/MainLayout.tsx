import { Outlet, useNavigate } from 'react-router-dom';
import { useAuthStore } from '@/store/authStore';
import { useCartStore } from '@/store/cartStore';
import { apiClient } from '@/services/apiClient';
import { Search, Bell, ShoppingCart, LogOut, UserCircle, Settings } from 'lucide-react';

import { SidebarProvider, SidebarTrigger } from "@/components/ui/sidebar"
import { AppSidebar } from "./AppSidebar"
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { TooltipProvider } from '../ui/tooltip';

export default function MainLayout() {
  const { user, clearAuth } = useAuthStore();
  const { totalItems, clearCart } = useCartStore();
  const navigate = useNavigate();

  const handleLogout = async () => {
    try {
      await apiClient.post('/auth/logout');
    } finally {
      clearCart();
      clearAuth();
      navigate('/login', { replace: true });
    }
  };

  const userInitial = user?.name ? user.name.charAt(0).toUpperCase() : 'N';

  return (
    <TooltipProvider>
        <SidebarProvider>
        {/* 1. The Dynamic Shadcn Sidebar */}
        <AppSidebar />
        
        {/* 2. Main Content Wrapper */}
        <div className="flex-1 flex flex-col min-w-0 h-screen overflow-hidden bg-background">
            
            {/* TOP NAVBAR */}
            <header className="h-16 bg-white border-b border-border flex items-center justify-between px-4 sm:px-6 shrink-0">
            
            <div className="flex items-center flex-1 gap-4">
                {/* Shadcn trigger automatically opens/closes the sidebar */}
                <SidebarTrigger className="text-foreground hover:text-primary" />
                
                <div className="relative w-full max-w-md hidden sm:block">
                <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
                <Input 
                    type="search" 
                    placeholder="Search for dishes, categories..." 
                    className="w-full pl-9 bg-background/50 focus-visible:ring-primary"
                />
                </div>
            </div>

            {/* Right Actions & Profile */}
            <div className="flex items-center gap-3 sm:gap-5">
                <Button variant="ghost" size="icon" className="relative text-foreground hover:text-primary">
                <Bell className="w-5 h-5" />
                </Button>

                {user?.role !== 'Admin' && (
                <Button variant="ghost" size="icon" className="relative text-foreground hover:text-primary" onClick={() => navigate('/cart')}>
                    <ShoppingCart className="w-5 h-5" />
                    {totalItems > 0 && (
                    <span className="absolute -top-1 -right-1 bg-primary text-white text-[10px] font-bold px-1.5 py-0.5 rounded-full">
                        {totalItems}
                    </span>
                    )}
                </Button>
                )}

                <DropdownMenu>
                <DropdownMenuTrigger asChild>
                    <Button variant="ghost" className="relative h-9 w-9 rounded-full">
                    <Avatar className="h-9 w-9 border border-border">
                        {/* <AvatarImage src={user?.profileImageUrl || ""} alt="Profile" /> */}
                        <AvatarFallback className="bg-primary/10 text-primary font-bold">
                        {userInitial}
                        </AvatarFallback>
                    </Avatar>
                    </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent className="w-56" align="end" forceMount>
                    <DropdownMenuLabel className="font-normal">
                    <div className="flex flex-col space-y-1">
                        <p className="text-sm font-medium leading-none">{user?.name}</p>
                        <p className="text-xs leading-none text-muted-foreground">{user?.email}</p>
                        {/* Add a tiny badge to show their role */}
                        <p className="text-xs font-bold text-primary mt-1">{user?.role}</p>
                    </div>
                    </DropdownMenuLabel>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem onClick={() => navigate('/profile')} className="cursor-pointer">
                    <UserCircle className="mr-2 h-4 w-4" />
                    <span>My Profile</span>
                    </DropdownMenuItem>
                    <DropdownMenuItem className="cursor-pointer">
                    <Settings className="mr-2 h-4 w-4" />
                    <span>Settings</span>
                    </DropdownMenuItem>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem onClick={handleLogout} className="cursor-pointer text-red-600 focus:text-red-600 focus:bg-red-50">
                    <LogOut className="mr-2 h-4 w-4" />
                    <span>Log out</span>
                    </DropdownMenuItem>
                </DropdownMenuContent>
                </DropdownMenu>
            </div>
            </header>

            {/* PAGE CONTENT */}
            <main className="flex-1 overflow-y-auto p-4 sm:p-6 lg:p-8">
            <Outlet />
            </main>
        </div>
        </SidebarProvider>
    </TooltipProvider>
  );
}