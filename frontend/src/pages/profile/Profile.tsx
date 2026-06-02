import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { userApi } from '@/services/userApi';
import { updateProfileSchema, type UpdateProfileFormData } from '@/schemas/user';
import { toast } from 'sonner';
import { Camera, Loader2 } from 'lucide-react';
import { getApiErrorMessage } from '@/lib/utils';
import { useAuth } from '@/hooks/useAuth';

import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { InputField } from '@/components/common';
import { LoadingButton } from '@/components/common/LoadingButton';

const ALLOWED_IMAGE_ORIGIN = 'https://res.cloudinary.com/drkdujqo5/image/upload/';

function sanitiseImageUrl(url: string | null | undefined): string {
  if (!url) return '';
  return url.startsWith(ALLOWED_IMAGE_ORIGIN) ? url : '';
}

export default function Profile() {
  const { user, setAuth, accessToken } = useAuth();

  const [isLoadingProfile, setIsLoadingProfile] = useState(true);
  const [isSavingInfo, setIsSavingInfo] = useState(false);
  const [isUploadingImage, setIsUploadingImage] = useState(false);

  const [previewUrl, setPreviewUrl] = useState<string | null>(null);

  const { register, handleSubmit, reset, formState: { errors, isDirty } } = useForm<UpdateProfileFormData>({
    resolver: zodResolver(updateProfileSchema),
    defaultValues: {
      name: '',
      phone: '',
    },
  });

  useEffect(() => {
    let isMounted = true;

    const fetchTrueProfile = async () => {
      try {
        const { data: trueProfile } = await userApi.getProfile();

        if (!isMounted) return;

        if (accessToken) setAuth(trueProfile, accessToken);

        reset({
          name: trueProfile.name,
          phone: trueProfile.phone ?? '',
        });
      } catch (error: unknown) {
        toast.error(getApiErrorMessage(error, 'Failed to sync profile data'));
      } finally {
        if (isMounted) setIsLoadingProfile(false);
      }
    };

    fetchTrueProfile();

    return () => { isMounted = false; };
  }, [reset, setAuth, accessToken]);

  useEffect(() => {
    return () => {
      if (previewUrl && previewUrl.startsWith('blob:')) {
        URL.revokeObjectURL(previewUrl);
      }
    };
  }, [previewUrl]);

  const userInitial = user?.name ? user.name.charAt(0).toUpperCase() : '?';
  const isBusy = isSavingInfo || isUploadingImage || isLoadingProfile;

  const onSubmitInfo = async (data: UpdateProfileFormData) => {
    setIsSavingInfo(true);
    try {
      const { data: updatedUser } = await userApi.updateProfile(data);
      if (accessToken) setAuth(updatedUser, accessToken);

      reset({
        name: updatedUser.name,
        phone: updatedUser.phone ?? '',
      });
      toast.success('Profile saved');
    } catch (error: unknown) {
      toast.error(getApiErrorMessage(error, 'Failed to save profile'));
    } finally {
      setIsSavingInfo(false);
    }
  };

  const handleImageUpload = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];
    if (!allowedTypes.includes(file.type)) {
      toast.error('Only JPEG, PNG, or WebP images are allowed');
      event.target.value = '';
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      toast.error('Image must be under 5 MB');
      event.target.value = '';
      return;
    }

    const localBlob = URL.createObjectURL(file);
    setPreviewUrl(localBlob);
    setIsUploadingImage(true);

    try {
      const { data: updatedProfile } = await userApi.uploadProfileImage(file);
      if (accessToken) setAuth(updatedProfile, accessToken);

      setPreviewUrl(null);
      toast.success('Photo updated');
    } catch (error: unknown) {
      setPreviewUrl(null);
      toast.error(getApiErrorMessage(error, 'Failed to upload photo'));
    } finally {
      setIsUploadingImage(false);
      event.target.value = '';
    }
  };

  if (isLoadingProfile || !user) {
    return (
      <div className="max-w-4xl mx-auto space-y-6">
        <div className="h-8 w-48 bg-muted animate-pulse rounded" />
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div className="col-span-1 h-64 bg-muted animate-pulse rounded-lg" />
          <div className="col-span-2 h-64 bg-muted animate-pulse rounded-lg" />
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight text-foreground">My Profile</h1>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">

        {/* AVATAR UPLOAD CARD */}
        <Card className="col-span-1 border-border bg-white shadow-sm">
          <CardHeader>
            <CardTitle className="text-lg">Avatar Image</CardTitle>
            <CardDescription>Click to update your photo.</CardDescription>
          </CardHeader>
          <CardContent className="flex flex-col items-center pb-8">
            <div className="relative group cursor-pointer">
              <Avatar className="h-32 w-32 border-4 border-background shadow-md">
                <AvatarImage
                  src={previewUrl ?? sanitiseImageUrl(user.profileImageUrl)}
                  alt={user.name}
                  className="object-cover"
                />
                <AvatarFallback className="bg-primary/10 text-primary text-4xl font-bold">
                  {userInitial}
                </AvatarFallback>
              </Avatar>

              <input
                type="file"
                id="picture-upload"
                className="hidden"
                accept="image/jpeg, image/png, image/webp"
                onChange={handleImageUpload}
                disabled={isBusy}
              />

              <label
                htmlFor="picture-upload"
                tabIndex={isBusy ? -1 : 0}
                onKeyDown={(e) => {
                  if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    document.getElementById('picture-upload')?.click();
                  }
                }}
                className={`absolute inset-0 flex items-center justify-center bg-black/50 rounded-full transition-opacity cursor-pointer focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2 ${
                  isUploadingImage ? 'opacity-100' : 'opacity-0 group-hover:opacity-100 focus:opacity-100'
                }`}
              >
                {isUploadingImage ? (
                  <Loader2 className="w-8 h-8 text-white animate-spin" />
                ) : (
                  <Camera className="w-8 h-8 text-white" />
                )}
              </label>
            </div>
            <p className="text-xs text-muted-foreground mt-4 text-center">
              Supports JPEG, PNG, or WebP up to 5 MB.
            </p>
          </CardContent>
        </Card>

        {/* PERSONAL DETAILS CARD */}
        <Card className="col-span-1 md:col-span-2 border-border bg-white shadow-sm">
          <CardHeader>
            <CardTitle className="text-lg">Personal Details</CardTitle>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit(onSubmitInfo)} className="space-y-4" noValidate>

              <InputField
                label="Full Name"
                htmlFor="name"
                error={errors.name}
                register={register('name')}
                inputClassName="bg-background focus-visible:ring-primary"
              />

              <InputField
                label="Phone Number"
                htmlFor="phone"
                placeholder="+91XXXXXXXXXX"
                error={errors.phone}
                register={register('phone')}
                inputClassName="bg-background focus-visible:ring-primary"
              />

              <div className="space-y-2">
                <Label htmlFor="email">Email Address</Label>
                <Input
                  id="email"
                  type="email"
                  value={user.email}
                  readOnly
                  className="bg-muted cursor-not-allowed text-muted-foreground"
                />
                <p className="text-xs text-muted-foreground">Email address cannot be changed.</p>
              </div>

              <div className="pt-4 flex justify-end">
                <LoadingButton
                  type="submit"
                  disabled={isBusy || !isDirty}
                  isLoading={isSavingInfo}
                  loadingText="Saving..."
                  className="w-full sm:w-auto bg-primary text-primary-foreground hover:bg-primary/90"
                >
                  Save Changes
                </LoadingButton>
              </div>
            </form>
          </CardContent>
        </Card>

      </div>
    </div>
  );
}
