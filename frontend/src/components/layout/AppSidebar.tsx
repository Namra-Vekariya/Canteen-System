import { Link, useLocation } from "react-router-dom"
import { Coffee, LayoutDashboard, ListOrdered, Users } from "lucide-react"
import { useAuthStore } from "@/store/authStore"

import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
} from "@/components/ui/sidebar"

export function AppSidebar() {
  const { user } = useAuthStore();
  const location = useLocation();

  // 1. Define routes based on the Role from your JWT
  const navLinks = user?.role === 'Admin' 
    ? [
        { title: 'Dashboard', url: '/dashboard', icon: LayoutDashboard },
        { title: 'Menu Config', url: '/admin/menu', icon: Coffee },
        { title: 'Order Queue', url: '/admin/orders', icon: ListOrdered },
        { title: 'Users', url: '/admin/users', icon: Users },
      ]
    : [
        { title: 'Today\'s Menu', url: '/dashboard', icon: Coffee },
        { title: 'My Orders', url: '/orders', icon: ListOrdered },
      ];

  return (
    <Sidebar>
      <SidebarHeader className="h-16 flex items-center justify-center border-b border-border">
        <div className="flex items-center gap-2 font-bold text-xl text-primary">
          <div className="w-8 h-8 bg-primary rounded-md flex items-center justify-center text-white">
            C
          </div>
          CanteenGo
        </div>
      </SidebarHeader>
      
      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupLabel>Application</SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu>
              {navLinks.map((item) => {
                const isActive = location.pathname === item.url;
                return (
                  <SidebarMenuItem key={item.title}>
                    <SidebarMenuButton asChild isActive={isActive} tooltip={item.title}>
                      <Link to={item.url}>
                        <item.icon />
                        <span>{item.title}</span>
                      </Link>
                    </SidebarMenuButton>
                  </SidebarMenuItem>
                );
              })}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>
    </Sidebar>
  )
}