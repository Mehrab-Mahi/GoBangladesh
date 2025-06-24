var appClient = {
    get: function (url, data, callBack) {
        this.httpClient(url, 'GET', data, callBack)
    },

    post: function (url, data, callBack) {
        this.httpClient(url, 'POST', data, callBack)
    },

    put: function (url, data, callBack) {
        this.httpClient(url, 'PUT', data, callBack)
    },
    deffered: function (url, data, callBack) {
        return $.get(url, data);
    },
    httpClient: function (url, type, data, callBack) {
        $.ajax({
            url: url,
            type: type,
            dataType: "json",
            headers: {
                "Content-Type": "application/json",
                "Authorization": "Bearer " + window.localStorage.getItem("accessToken")
            },
            data: JSON.stringify(data),
            success: function (response) {
                callBack(response);
            },
            error: function (e) {
                // if (e.status === 401) {
                window.location.href = '/account/logout';
                // }
            }
        });
    }
}