import { useEffect, useState } from 'react';
import { useCartStore } from '@/store/cartStore';
import { Card, CardContent, CardFooter } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Switch } from '@/components/ui/switch';
import { Label } from '@/components/ui/label';
import { ShoppingCart, Clock, Flame, Leaf, Drumstick } from 'lucide-react';
import { format } from 'date-fns';
import { menuApi, type CategoryResponse, type MenuItemResponse } from '@/services/menuApi';
import { useAuth } from '@/hooks/useAuth';
import { getApiErrorMessage } from '@/lib/utils';
import fallbackImage from '@/assets/images/thali.jpg';
import { useNavigate } from 'react-router-dom';

export default function Dashboard() {
    const navigate = useNavigate();
    const { user } = useAuth();
    const [categories, setCategories] = useState<CategoryResponse[]>([]);
    const [menuItems, setMenuItems] = useState<MenuItemResponse[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const [activeCategoryId, setActiveCategoryId] = useState<string | null>(null);
    const [vegOnly, setVegOnly] = useState(false);

    const { items: cartItems, addItem, updateQuantity, totalItems } = useCartStore();

    const fetchDashboardData = async () => {
        setLoading(true);
        setError(null);
        try {
            const [catsResult, itemsResult] = await Promise.allSettled([
                menuApi.getCategories(),
                menuApi.getMenuItems()
            ]);

            if (catsResult.status === 'fulfilled') {
                setCategories(catsResult.value);
            }
            if (itemsResult.status === 'fulfilled') {
                setMenuItems(itemsResult.value);
            }

            const bothFailed = catsResult.status === 'rejected' && itemsResult.status === 'rejected';
            if (bothFailed) {
                setError("Failed to load menu data. Please try again.");
            } else if (catsResult.status === 'rejected') {
                setError("Categories failed to load. Showing all items.");
            } else if (itemsResult.status === 'rejected') {
                setError("Menu items failed to load. Please try again.");
            }
        } catch (err) {
            setError(getApiErrorMessage(err, "Failed to load menu data. Please try again."));
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchDashboardData();
    }, []);

    const filteredItems = menuItems.filter(item => {
        if (activeCategoryId && item.categoryId !== activeCategoryId) return false;
        if (vegOnly && !item.isVeg) return false;
        return true;
    });

    if (loading) {
        return <div className="flex h-screen items-center justify-center text-orange-500">Loading menu...</div>;
    }

    if (error) {
        return (
            <div className="flex h-screen items-center justify-center flex-col gap-4">
                <p className="text-red-500 text-lg">{error}</p>
                <Button onClick={fetchDashboardData} className="bg-orange-500 hover:bg-orange-600 text-white">
                    Retry
                </Button>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-gray-50 flex flex-col min-w-0 w-full">
            <header className="bg-orange-500 text-white px-4 sm:px-8 py-4 sm:py-6 shadow-md flex justify-between items-center rounded-t-lg">
                <div className="min-w-0">
                    <h1 className="text-xl sm:text-3xl font-bold tracking-tight truncate">Good afternoon! {user?.name}</h1>
                    <p className="text-orange-100 mt-1 text-sm sm:text-base hidden sm:block">Today's menu is ready. What are you craving?</p>
                </div>
                <div className="flex items-center gap-3 sm:gap-6 shrink-0">
                    <div className="text-right hidden md:block">
                        <p className="text-sm font-medium text-orange-100">{format(new Date(), 'EEEE, dd MMM yyyy')}</p>
                    </div>
                    <Button onClick={() => navigate('/cart')} variant="secondary" className="bg-white text-orange-600 hover:bg-orange-50 relative">
                        <ShoppingCart className="w-5 h-5 mr-2" />
                        Cart
                        {totalItems > 0 && (
                            <span className="absolute -top-2 -right-2 bg-red-600 text-white text-xs font-bold rounded-full w-6 h-6 flex items-center justify-center">
                                {totalItems}
                            </span>
                        )}
                    </Button>
                </div>
            </header>

            <main className="flex-1 w-full mx-auto p-4 sm:p-6 flex gap-8">
                <aside className="flex-shrink-0 space-y-8 hidden md:block">
                    <div>
                        <h3 className="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-4">Categories</h3>
                        <ul className="space-y-2">
                            <li>
                                <button
                                    onClick={() => setActiveCategoryId(null)}
                                    className={`w-full text-left px-4 py-2 rounded-md font-medium transition-colors ${
                                        activeCategoryId === null
                                            ? 'bg-orange-100 text-orange-700'
                                            : 'text-gray-600 hover:bg-gray-100'
                                    }`}
                                >
                                    All Items
                                </button>
                            </li>
                            {categories.map(cat => (
                                <li key={cat.id}>
                                    <button
                                        onClick={() => setActiveCategoryId(cat.id)}
                                        className={`w-full text-left px-4 py-2 rounded-md font-medium transition-colors ${
                                            activeCategoryId === cat.id
                                                ? 'bg-orange-100 text-orange-700'
                                                : 'text-gray-600 hover:bg-gray-100'
                                        }`}
                                    >
                                        {cat.name}
                                    </button>
                                </li>
                            ))}
                        </ul>
                    </div>

                    <div>
                        <h3 className="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-4">Filters</h3>
                        <div className="flex items-center space-x-2 bg-white p-4 rounded-lg shadow-sm border border-gray-100">
                            <Switch
                                id="veg-mode"
                                checked={vegOnly}
                                onCheckedChange={setVegOnly}
                                className="data-[state=checked]:bg-green-600"
                            />
                            <Label htmlFor="veg-mode" className="font-medium cursor-pointer flex items-center gap-2">
                                <Leaf className="w-4 h-4 text-green-600" />
                                Veg Only
                            </Label>
                        </div>
                    </div>
                </aside>

                <section className="flex-1">
                    <div className="mb-6 flex justify-between items-end border-b pb-4">
                        <h2 className="text-2xl font-bold text-gray-800">
                            {activeCategoryId ? categories.find(c => c.id === activeCategoryId)?.name : 'All Menu Items'}
                        </h2>
                        <span className="text-sm text-gray-500 font-medium">{filteredItems.length} items</span>
                    </div>

                    <div className="mb-6 md:hidden">
                        <div className="flex items-center gap-3 mb-3">
                            <Switch
                                id="veg-mode-mobile"
                                checked={vegOnly}
                                onCheckedChange={setVegOnly}
                                className="data-[state=checked]:bg-green-600"
                            />
                            <Label htmlFor="veg-mode-mobile" className="font-medium cursor-pointer flex items-center gap-2 text-sm">
                                <Leaf className="w-4 h-4 text-green-600" />
                                Veg Only
                            </Label>
                        </div>
                        <div className="flex gap-2 overflow-x-auto pb-2">
                            <button
                                onClick={() => setActiveCategoryId(null)}
                                className={`flex-shrink-0 px-4 py-2 rounded-full text-sm font-medium transition-colors ${
                                    activeCategoryId === null
                                        ? 'bg-orange-500 text-white'
                                        : 'bg-white text-gray-600 border border-gray-200 hover:bg-gray-50'
                                }`}
                            >
                                All
                            </button>
                            {categories.map(cat => (
                                <button
                                    key={cat.id}
                                    onClick={() => setActiveCategoryId(cat.id)}
                                    className={`flex-shrink-0 px-4 py-2 rounded-full text-sm font-medium transition-colors ${
                                        activeCategoryId === cat.id
                                            ? 'bg-orange-500 text-white'
                                            : 'bg-white text-gray-600 border border-gray-200 hover:bg-gray-50'
                                    }`}
                                >
                                    {cat.name}
                                </button>
                            ))}
                        </div>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                        {filteredItems.map(item => {
                            const cartItem = cartItems[item.id];
                            const quantity = cartItem?.quantity ?? 0;

                            return (
                                <Card key={item.id} className={`overflow-hidden border-0 shadow-sm hover:shadow-md transition-shadow ${!item.isAvailable && 'opacity-60 grayscale'}`}>
                                    <div className="h-36 sm:h-40 bg-orange-50 relative flex items-center justify-center overflow-hidden">
                                        <img
                                            src={item.imageUrl || fallbackImage}
                                            alt={item.name}
                                            className="w-full h-full object-cover"
                                            onError={(e) => {
                                                if (!e.currentTarget.dataset.fallback) {
                                                    e.currentTarget.dataset.fallback = 'true';
                                                    e.currentTarget.src = fallbackImage;
                                                } else {
                                                    e.currentTarget.style.display = 'none';
                                                }
                                            }}
                                        />
                                        <div className="absolute top-3 left-3 bg-white p-1 rounded shadow-sm">
                                            {item.isVeg ? <Leaf className="w-4 h-4 text-green-600" /> : <Drumstick className="w-4 h-4 text-red-600" />}
                                        </div>
                                        {!item.isAvailable && (
                                            <div className="absolute inset-0 bg-black/40 flex items-center justify-center">
                                                <span className="bg-white text-gray-900 text-sm font-bold px-3 py-1 rounded-full">Sold Out</span>
                                            </div>
                                        )}
                                    </div>

                                    <CardContent className="p-4">
                                        <h3 className="font-bold text-lg text-gray-900 truncate">{item.name}</h3>
                                        <p className="text-xs text-gray-500 mb-3">{item.categoryName}</p>

                                        <div className="flex items-center gap-4 text-xs font-medium text-gray-600">
                                            {item.prepTimeMins != null && (
                                                <span className="flex items-center gap-1"><Clock className="w-3 h-3" /> {item.prepTimeMins} min</span>
                                            )}
                                            {item.calories != null && (
                                                <span className="flex items-center gap-1"><Flame className="w-3 h-3" /> {item.calories}</span>
                                            )}
                                        </div>
                                    </CardContent>

                                    <CardFooter className="pt-4 flex flex-col sm:flex-row items-start sm:items-center justify-between gap-2">
                                        <span className="text-lg font-bold text-gray-900">₹{item.price}</span>

                                        {item.isAvailable ? (
                                            quantity > 0 ? (
                                                <div className="flex items-center gap-3 bg-orange-50 rounded-md border border-orange-200">
                                                    <button onClick={() => updateQuantity(item.id, -1)} className="w-6 h-6 flex items-center justify-center text-orange-600 hover:bg-orange-100 rounded">-</button>
                                                    <span className="font-semibold text-sm w-4 text-center">{quantity}</span>
                                                    <button onClick={() => updateQuantity(item.id, 1)} className="w-6 h-6 flex items-center justify-center text-orange-600 hover:bg-orange-100 rounded">+</button>
                                                </div>
                                            ) : (
                                                <Button
                                                    onClick={() => addItem(item)}
                                                    variant="outline"
                                                    className="border-orange-500 text-orange-600 hover:bg-orange-50"
                                                    size="sm"
                                                >
                                                    Add
                                                </Button>
                                            )
                                        ) : (
                                            <Button disabled variant="outline" size="sm">Unavailable</Button>
                                        )}
                                    </CardFooter>
                                </Card>
                            );
                        })}
                    </div>
                    {filteredItems.length === 0 && (
                        <div className="text-center py-20 text-gray-500">
                            No items found for the selected criteria.
                        </div>
                    )}
                </section>
            </main>
        </div>
    );
}
