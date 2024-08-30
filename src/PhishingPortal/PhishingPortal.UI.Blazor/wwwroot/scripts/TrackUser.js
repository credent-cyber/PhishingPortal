async function getClientDetails(dotNetObject) {
    try {
        // Get the browser and OS details
        const userAgent = navigator.userAgent;
        const browserInfo = getBrowserInfo(userAgent);
        const osInfo = getOSInfo(userAgent);

        // Fetch IP address and geolocation
        const ipAddress = await getIpAddress();
        const geoLocation = await getGeoLocation();

        const info = {
            ip: ipAddress,
            latitude: geoLocation.latitude,
            longitude: geoLocation.longitude,
            city: geoLocation.city,
            region: geoLocation.region,
            country: geoLocation.country,
            browser: browserInfo,
            os: osInfo
        };

        dotNetObject.invokeMethodAsync('ReceiveClientDetails', info);
    } catch (error) {
        console.error('Error fetching client details:', error);
        dotNetObject.invokeMethodAsync('ReceiveClientDetails', {
            ip: 'Error',
            latitude: 'Error',
            longitude: 'Error',
            city: 'Error',
            region: 'Error',
            country: 'Error',
            browser: 'Error',
            os: 'Error'
        });
    }
}

function getBrowserInfo(userAgent) {
    if (userAgent.indexOf('Firefox') > -1) {
        return 'Mozilla Firefox';
    } else if (userAgent.indexOf('SamsungBrowser') > -1) {
        return 'Samsung Internet';
    } else if (userAgent.indexOf('Opera') > -1 || userAgent.indexOf('OPR') > -1) {
        return 'Opera';
    } else if (userAgent.indexOf('Trident/') > -1) {
        return 'Internet Explorer';
    } else if (userAgent.indexOf('Edge/') > -1) {
        return 'Microsoft Edge';
    } else if (userAgent.indexOf('Chrome') > -1) {
        return 'Google Chrome';
    } else if (userAgent.indexOf('Safari') > -1) {
        return 'Safari';
    } else {
        return 'Unknown';
    }
}

function getOSInfo(userAgent) {
    if (userAgent.indexOf('Win') > -1) return 'Windows';
    if (userAgent.indexOf('Mac') > -1) return 'Mac OS';
    if (userAgent.indexOf('X11') > -1) return 'Unix';
    if (userAgent.indexOf('Linux') > -1) return 'Linux';
    if (userAgent.indexOf('Android') > -1) return 'Android';
    if (userAgent.indexOf('like Mac') > -1) return 'iOS';
    return 'Unknown';
}

async function getIpAddress() {
    try {
        const response = await fetch('https://api.ipify.org?format=json');
        const data = await response.json();
        return data.ip;
    } catch (error) {
        console.error('Error fetching IP address:', error);
        return 'Error';
    }
}

async function getGeoLocation() {
    return new Promise((resolve, reject) => {
        if (!navigator.geolocation) {
            console.error('Geolocation is not supported by this browser.');
            resolve({
                latitude: 'Error',
                longitude: 'Error',
                city: 'Error',
                region: 'Error',
                country: 'Error'
            });
            return;
        }

        navigator.geolocation.getCurrentPosition(
            async (position) => {
                try {
                    const { latitude, longitude } = position.coords;

                    // Use OpenStreetMap's Nominatim API for reverse geocoding
                    const response = await fetch(`https://nominatim.openstreetmap.org/reverse?format=json&lat=${latitude}&lon=${longitude}&addressdetails=1`);
                    const data = await response.json();

                    resolve({
                        latitude: latitude.toString(),
                        longitude: longitude.toString(),
                        city: data.address.city || 'Unknown',
                        region: data.address.state || 'Unknown',
                        country: data.address.country || 'Unknown'
                    });
                } catch (error) {
                    console.error('Error fetching geolocation details:', error);
                    resolve({
                        latitude: 'Error',
                        longitude: 'Error',
                        city: 'Error',
                        region: 'Error',
                        country: 'Error'
                    });
                }
            },
            (error) => {
                console.error('Error getting geolocation:', error);
                resolve({
                    latitude: 'Error',
                    longitude: 'Error',
                    city: 'Error',
                    region: 'Error',
                    country: 'Error'
                });
            }
        );
    });
}
