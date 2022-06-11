import React, { useCallback, useState } from 'react';
import { useTranslation } from 'react-i18next';
import Information from './shippingTabs/information';
import Routes from './shippingTabs/routes';
import Documents from './shared/documents';
import History from './shared/history';
import Accounts from './shippingTabs/accounts';
import { useSelector } from 'react-redux';
import { userPermissionsSelector } from '../../../ducks/profile';
import CardLayout from '../../../components/CardLayout';
import Costs from './shippingTabs/costs';
import CreateShipping from "./shippingTabs/createShipping";

const ShippingCard = ({
    form,
    onChangeForm,
    name,
    id,
    onClose: beforeClose,
    settings,
    title,
    actionsFooter,
    onClose,
    actionsHeader,
    loading,
    load,
    error,
}) => {
    const { t } = useTranslation();
    const userPermissions = useSelector(state => userPermissionsSelector(state));
    const { orders = [] } = form;
    let [routeActiveIndex, setRouteActiveIndex] = useState(0);

    const handleTabChange = useCallback((e, { activeIndex }) => {
        setRouteActiveIndex(activeIndex);
    }, []);

    const getPanes = () => {
        const obj = [
            {
                menuItem: t('information'),
                render: () => (
                    <Information
                        form={form}
                        onChange={onChangeForm}
                        error={error}
                        settings={settings}
                    />
                ),
            },
            {
                menuItem: t('route'),
                render: () => (
                    <Routes
                        form={form}
                        routeActiveIndex={routeActiveIndex}
                        settings={settings}
                        tabChange={handleTabChange}
                        onChange={onChangeForm}
                    />
                ),
            },
            {
                menuItem: t('costs'),
                render: () => (
                    <Costs form={form} settings={settings} error={error} onChange={onChangeForm} />
                ),
            },
        ];

        if (userPermissions.includes(10) || userPermissions.includes(11)) {
            obj.push({
                menuItem: t('documents'),
                render: () => (
                    <Documents
                        gridName={name}
                        cardId={id}
                        load={load}
                        isEditPermissions={userPermissions.includes(11)}
                    />
                ),
            });
        }

        if (userPermissions.includes(12)) {
            obj.push({
                menuItem: t('history'),
                render: () => <History cardId={id} status={form.status} />,
            });
        }

        return obj;
    };

    return (
        <>
            {id ? (
                <CardLayout
                    title={title}
                    actionsFooter={actionsFooter}
                    actionsHeader={actionsHeader}
                    content={getPanes}
                    onClose={onClose}
                    loading={loading}
                />
            ) : (
                <CardLayout
                    title={title}
                    actionsFooter={actionsFooter}
                    onClose={onClose}
                    loading={loading}
                >
                    <CreateShipping form={form} error={error} settings={settings} onChange={onChangeForm} />
                </CardLayout>
            )}
        </>
    );
};

export default ShippingCard;
