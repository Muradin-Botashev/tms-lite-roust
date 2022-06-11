import React, { useCallback, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import {useDispatch, useSelector} from 'react-redux';
import {Button, Confirm, Form} from 'semantic-ui-react';
import FormField from '../../components/BaseComponents';
import {CHECKBOX_TYPE, PASSWORD_TYPE, TEXT_TYPE} from '../../constants/columnTypes';
import {
    addError,
    editProfileSettingsRequest,
    errorSelector,
    getNotificationsListRequest,
    getProfileSettingsRequest,
    notificationsSelector,
    profileSettingsSelector,
    progressEditSelector,
} from '../../ducks/profile';
import CardLayout from '../../components/CardLayout';

const Profile = props => {
    const {history} = props;
    const {t} = useTranslation();
    const dispatch = useDispatch();
    let [notChangeForm, setNotChangeForm] = useState(true);
    let [confirmation, setConfirmation] = useState({open: false});
    let [form, setForm] = useState({});

    const profile = useSelector(state => profileSettingsSelector(state));

    useEffect(() => {
        dispatch(getProfileSettingsRequest());
        dispatch(getNotificationsListRequest());
    }, []);

    useEffect(
        () => {
            setForm({...profile});
        },
        [profile],
    );

    const progressEdit = useSelector(state => progressEditSelector(state));
    const notificationsList = useSelector(state => notificationsSelector(state));

    const error = useSelector(state => errorSelector(state));

    const handleChange = useCallback((e, {name, value}) => {
        if (notChangeForm) {
            setNotChangeForm(false);
        }
        setForm(prevState => ({
            ...prevState,
            [name]: value,
        }));
    }, [notChangeForm, form]);

    const handleSave = () => {
        if (handleComparePassword()) {
            dispatch(
                editProfileSettingsRequest({
                    form,
                    callbackSuccess: onClose,
                }),
            );
        }
    };

    const handleComparePassword = () => {
        const { returnNewPassword, newPassword } = form;

        if (returnNewPassword !== newPassword) {
            dispatch(
                addError({
                    name: 'returnNewPassword',
                    message: t('passwords_do_not_match'),
                }),
            );
            return false;
        }

        return true;
    };

    const handleNotificationsChange = (e, { value, item }) => {
        if (notChangeForm) {
            setNotChangeForm(false);
        }
        if (value) {
            setForm(form => ({
                ...form,
                notifications: [...(form.notifications || []), item],
            }));
        } else {
            setForm(form => ({
                ...form,
                notifications: form.notifications
                    ? form.notifications.filter(notification => notification.value !== item.value)
                    : null,
            }));
        }
    };

    const confirmClose = () => {
        setConfirmation({open: false});
    };

    const onClose = () => {
        setForm({});
        history.goBack();
    };

    const handleClose = () => {
        if (notChangeForm) {
            onClose();
        } else {
            setConfirmation({
                open: true,
                content: t('confirm_close_dictionary'),
                onCancel: confirmClose,
                onConfirm: onClose,
            });
        }
    };

    const getActionsFooter = useCallback(
        () => {
            return (
                <>
                    <Button color="grey" onClick={handleClose}>
                        {t('CancelButton')}
                    </Button>
                    <Button color="blue" disabled={notChangeForm} onClick={handleSave}>
                        {t('SaveButton')}
                    </Button>
                </>
            );
        },
        [form, notChangeForm],
    );

    const getContent = useCallback(
        () => {
            return [
                {
                    menuItem: 'general',
                    render: () => (
                        <Form className="profile-form">
                            <FormField
                                name="userName"
                                type={TEXT_TYPE}
                                value={form['userName']}
                                isRequired
                                error={error['userName']}
                                onChange={handleChange}
                            />
                            <FormField
                                name="email"
                                type={TEXT_TYPE}
                                value={form['email']}
                                isRequired
                                error={error['email']}
                                onChange={handleChange}
                            />
                            <FormField
                                name="role"
                                type={TEXT_TYPE}
                                value={form['roleName']}
                                isReadOnly
                                onChange={handleChange}
                            />
                        </Form>
                    ),
                },
                {
                    menuItem: 'Password Change',
                    render: () => (
                        <Form className="profile-form">
                            <FormField
                                name="oldPassword"
                                type={PASSWORD_TYPE}
                                value={form['oldPassword']}
                                error={error['oldPassword']}
                                onChange={handleChange}
                            />
                            <FormField
                                name="newPassword"
                                type={PASSWORD_TYPE}
                                error={error['newPassword']}
                                value={form['newPassword']}
                                onChange={handleChange}
                            />
                            <FormField
                                name="returnNewPassword"
                                type={PASSWORD_TYPE}
                                value={form['returnNewPassword']}
                                error={error['returnNewPassword']}
                                onChange={handleChange}
                                onBlur={handleComparePassword}
                            />
                            <Confirm
                                dimmer="blurring"
                                open={confirmation.open}
                                onCancel={confirmation.onCancel}
                                cancelButton={t('cancelConfirm')}
                                confirmButton={t('Yes')}
                                onConfirm={confirmation.onConfirm}
                                content={confirmation.content}
                            />
                        </Form>
                    ),
                },
                {
                    menuItem: 'Set up notifications',
                    render: () => (
                        <Form className="profile-form">
                            {notificationsList.map(item => (
                                <FormField
                                    key={item.value}
                                    name={item.name}
                                    type={CHECKBOX_TYPE}
                                    checked={
                                        form['notifications'] &&
                                        form['notifications']
                                            .map(item => item.value)
                                            .includes(item.value)
                                    }
                                    item={item}
                                    onChange={(e, { value }) =>
                                        handleNotificationsChange(e, { item, value })
                                    }
                                />
                            ))}
                        </Form>
                    ),
                },
            ];
        },
        [form, error, confirmation],
    );

    return (
        <CardLayout
            title={t('profile_settings')}
            actionsFooter={getActionsFooter}
            content={getContent}
            onClose={handleClose}
        />
    );
};

export default Profile;
