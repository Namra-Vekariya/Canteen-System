import { useNavigate } from 'react-router-dom';
import { useCartStore } from '@/store/cartStore';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Trash2, ArrowLeft, ShoppingBag } from 'lucide-react';
import fallbackImage from '@/assets/images/thali.jpg';

export default function Cart() {
    const navigate = useNavigate();
    const { items, totalPrice, totalItems, updateQuantity, removeItem } = useCartStore();

    const cartItemsArray = Object.values(items);

    if (cartItemsArray.length === 0) {
        return (
            <div className="flex flex-col items-center justify-center p-6 min-h-[50vh]">
                <ShoppingBag className="w-24 h-24 text-gray-300 mb-6" />
                <h2 className="text-2xl font-bold text-gray-800 mb-2">Your cart is empty</h2>
                <p className="text-gray-500 mb-8">Looks like you haven't added anything to your cart yet.</p>
                <Button
                    onClick={() => navigate('/dashboard')}
                    className="bg-orange-500 hover:bg-orange-600 text-white px-8 py-6 rounded-full text-lg font-semibold"
                >
                    Browse Menu
                </Button>
            </div>
        );
    }

    return (
        <div className="bg-gray-50">
            {/* Header */}
            <header className="bg-white border-b border-gray-200 px-4 sm:px-8 py-4 flex items-center top-0 z-10">
                <button onClick={() => navigate(-1)} className="p-2 hover:bg-gray-100 rounded-full mr-4 transition-colors">
                    <ArrowLeft className="w-5 h-5 text-gray-600" />
                </button>
                <h1 className="text-lg sm:text-xl font-bold text-gray-900">Checkout ({cartItemsArray.length} items)</h1>
            </header>

            <main className="max-w-5xl mx-auto mt-6 px-4 sm:px-6 grid grid-cols-1 lg:grid-cols-3 gap-6 lg:gap-8">

                {/* Left: Order Details */}
                <div className="lg:col-span-2 space-y-4">
                    <h2 className="text-lg font-semibold text-gray-800">Order Details</h2>

                    {cartItemsArray.map(({ menuItem, quantity }) => (
                        <Card key={menuItem.id} className="border-0 shadow-sm overflow-hidden">
                            <CardContent className="p-3 sm:p-4">
                                <div className="flex items-start gap-3 sm:gap-4">
                                    {/* Image */}
                                    <div className="w-16 h-16 sm:w-20 sm:h-20 rounded-lg bg-orange-50 flex-shrink-0 overflow-hidden">
                                        <img
                                            src={menuItem.imageUrl || fallbackImage}
                                            alt={menuItem.name}
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
                                    </div>

                                    {/* Details */}
                                    <div className="flex-1 min-w-0">
                                        <div className="flex items-start justify-between gap-2">
                                            <div className="min-w-0">
                                                <h3 className="font-bold text-gray-900 text-base sm:text-lg truncate" title={menuItem.name}>
                                                    {menuItem.name}
                                                </h3>
                                                <p className="text-xs sm:text-sm text-gray-500">{menuItem.categoryName}</p>
                                            </div>
                                            <p className="font-bold text-gray-900 text-sm sm:text-base whitespace-nowrap">₹{menuItem.price * quantity}</p>
                                        </div>

                                        {/* Actions */}
                                        <div className="flex items-center justify-between mt-3">
                                            <div className="flex items-center gap-2 bg-gray-50 rounded-md p-1 border border-gray-200">
                                                <button
                                                    onClick={() => updateQuantity(menuItem.id, -1)}
                                                    className="w-7 h-7 sm:w-8 sm:h-8 flex items-center justify-center text-gray-600 hover:bg-gray-200 rounded font-bold text-sm"
                                                >
                                                    -
                                                </button>
                                                <span className="font-semibold text-sm w-6 text-center">{quantity}</span>
                                                <button
                                                    onClick={() => updateQuantity(menuItem.id, 1)}
                                                    className="w-7 h-7 sm:w-8 sm:h-8 flex items-center justify-center text-orange-600 hover:bg-orange-100 rounded font-bold text-sm"
                                                >
                                                    +
                                                </button>
                                            </div>
                                            <button
                                                onClick={() => removeItem(menuItem.id)}
                                                className="text-gray-400 hover:text-red-500 p-1.5 sm:p-2 rounded-full transition-colors"
                                            >
                                                <Trash2 className="w-4 h-4 sm:w-5 sm:h-5" />
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            </CardContent>
                        </Card>
                    ))}
                </div>

                {/* Right: Bill Summary */}
                <div className="lg:col-span-1 order-first lg:order-last">
                    <Card className="border-0 shadow-md lg:sticky lg:top-24">
                        <CardContent className="p-5 sm:p-6">
                            <h2 className="text-lg font-semibold text-gray-800 mb-5">Bill Summary</h2>

                            <div className="space-y-3 text-sm text-gray-600 mb-5">
                                {cartItemsArray.map(({ menuItem, quantity }) => (
                                    <div key={menuItem.id} className="flex justify-between gap-2">
                                        <span className="truncate">{menuItem.name} × {quantity}</span>
                                        <span className="font-medium text-gray-900 whitespace-nowrap">₹{menuItem.price * quantity}</span>
                                    </div>
                                ))}
                            </div>

                            <div className="border-t border-gray-100 pt-4 mb-5 flex justify-between items-center">
                                <span className="text-base font-bold text-gray-900">Total ({totalItems} items)</span>
                                <span className="text-xl font-bold text-orange-600">₹{totalPrice}</span>
                            </div>

                            <Button
                                onClick={() => alert('Checkout flow coming next!')}
                                className="w-full bg-orange-500 hover:bg-orange-600 text-white py-5 sm:py-6 text-base sm:text-lg font-bold rounded-xl"
                            >
                                Proceed to Checkout
                            </Button>
                        </CardContent>
                    </Card>
                </div>

            </main>
        </div>
    );
}
