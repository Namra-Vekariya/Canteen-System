import { useParams } from 'react-router-dom';
import { useNavigate } from 'react-router-dom';
import { CheckCircle2, ArrowLeft } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import fallbackImage from '@/assets/images/thali.jpg';
import { useApi } from '@/hooks/useApi';
import { orderApi } from '@/services/orderApi';

export default function OrderConfirmation() {
    const navigate = useNavigate();
    const { orderId } = useParams<{ orderId: string }>();

    const { data: order, isLoading, error } = useApi({
        queryFn: () => orderApi.getOrderById(orderId!),
        enabled: !!orderId,
        fallbackError: 'Order not found or you don\'t have access.',
    });

    if (isLoading) {
        return (
            <div className="min-h-screen bg-gray-50 flex items-center justify-center p-6">
                <div className="h-10 w-10 animate-spin rounded-full border-4 border-orange-500 border-t-transparent" />
            </div>
        );
    }

    if (error || !order) {
        return (
            <div className="min-h-screen bg-gray-50 flex items-center justify-center p-6">
                <Card className="max-w-md w-full border-0 shadow-md">
                    <CardContent className="p-8 text-center space-y-4">
                        <h2 className="text-xl font-bold text-gray-900">Order not found</h2>
                        <p className="text-gray-600 text-sm">
                            {error ?? 'We couldn\'t find the order details.'}
                        </p>
                        {orderId && (
                            <p className="text-xs text-gray-400 font-mono">Order ID: {orderId}</p>
                        )}
                        <Button onClick={() => navigate('/dashboard')} className="bg-orange-500 hover:bg-orange-600 text-white">
                            Back to Menu
                        </Button>
                    </CardContent>
                </Card>
            </div>
        );
    }

    const totalItems = order.items.reduce((sum, item) => sum + item.quantity, 0);
    const statusColor =
        order.status === 'Pending' ? 'bg-yellow-100 text-yellow-800' :
        order.status === 'Confirmed' ? 'bg-blue-100 text-blue-800' :
        order.status === 'Preparing' ? 'bg-orange-100 text-orange-800' :
        order.status === 'Ready' ? 'bg-green-100 text-green-800' :
        order.status === 'Collected' ? 'bg-gray-100 text-gray-800' :
        order.status === 'Cancelled' ? 'bg-red-100 text-red-800' :
        'bg-gray-100 text-gray-800';

    return (
        <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4 sm:p-6">
            <Card className="max-w-lg w-full border-0 shadow-lg">
                <CardContent className="p-6 sm:p-8 space-y-6">
                    <div className="text-center space-y-3">
                        <div className="inline-flex items-center justify-center w-20 h-20 sm:w-24 sm:h-24 bg-green-100 rounded-full">
                            <CheckCircle2 className="w-12 h-12 sm:w-14 sm:h-14 text-green-600" strokeWidth={2.5} />
                        </div>
                        <h1 className="text-2xl sm:text-3xl font-bold text-gray-900">Order Placed!</h1>
                        <p className="text-gray-600 text-sm sm:text-base">
                            Show this token at the counter to pay<br className="hidden sm:block" /> and collect your order.
                        </p>
                    </div>

                    <div className="flex items-center justify-center">
                        <div className="px-6 sm:px-8 py-4 border-2 border-dashed border-orange-300 bg-orange-50 rounded-xl">
                            <p className="text-2xl sm:text-3xl font-bold text-orange-700 font-mono tracking-wider text-center">
                                {order.orderNumber}
                            </p>
                        </div>
                    </div>

                    <div>
                        <h2 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-3">Order Summary</h2>
                        <div className="space-y-3">
                            {order.items.map((item, index) => (
                                <div key={index} className="flex items-center gap-3">
                                    <div className="w-12 h-12 sm:w-14 sm:h-14 rounded-lg bg-orange-50 flex-shrink-0 overflow-hidden">
                                        <img
                                            src={item.imageUrl || fallbackImage}
                                            alt={item.itemName}
                                            className="w-full h-full object-cover"
                                            onError={(e) => {
                                                if (!e.currentTarget.dataset.fallback) {
                                                    e.currentTarget.dataset.fallback = 'true';
                                                    e.currentTarget.src = fallbackImage;
                                                }
                                            }}
                                        />
                                    </div>
                                    <div className="flex-1 min-w-0">
                                        <p className="font-semibold text-gray-900 text-sm sm:text-base truncate">{item.itemName}</p>
                                        <p className="text-xs sm:text-sm text-gray-500">× {item.quantity}</p>
                                    </div>
                                    <p className="font-semibold text-gray-900 text-sm sm:text-base whitespace-nowrap">
                                        ₹{item.lineTotal.toFixed(2)}
                                    </p>
                                </div>
                            ))}
                        </div>
                        <div className="border-t border-gray-200 mt-4 pt-4 flex justify-between items-center">
                            <span className="font-bold text-gray-900">Total ({totalItems} items)</span>
                            <span className="text-xl font-bold text-orange-600">₹{order.totalAmount.toFixed(2)}</span>
                        </div>
                    </div>

                    <div className="space-y-2 bg-gray-50 rounded-lg p-4">
                        <div className="flex justify-between items-center text-sm">
                            <span className="text-gray-600">Payment Method</span>
                            <span className="font-semibold text-gray-900">{order.paymentMethod}</span>
                        </div>
                        <div className="flex justify-between items-center text-sm">
                            <span className="text-gray-600">Order Status</span>
                            <span className={`px-3 py-1 rounded-full text-xs font-semibold ${statusColor}`}>
                                {order.status}
                            </span>
                        </div>
                    </div>

                    <Button
                        onClick={() => navigate('/dashboard')}
                        className="w-full bg-orange-500 hover:bg-orange-600 text-white py-5 sm:py-6 text-base sm:text-lg font-bold rounded-xl"
                    >
                        <ArrowLeft className="w-4 h-4 mr-2" />
                        Back to Menu
                    </Button>
                </CardContent>
            </Card>
        </div>
    );
}
