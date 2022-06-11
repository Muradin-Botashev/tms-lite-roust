export default {
    logo: null,
    name: 'login_welcome',
    support_name: 'login_support',
    phoneNumber: '+7(495)737-39-55',
    form: {
        inputs: [
            {
                name: 'login',
                icon: 'user',
                type: 'text',
            },
            {
                name: 'password',
                icon: 'lock',
                type: 'password',
            },
        ],
        login_btn: {
            name: 'login_btn',
            api: {
                url: '/login',
                type: 'post',
            },
        },
    },
};
