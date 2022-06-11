import React, { Component } from 'react';
import { Form, Grid } from 'semantic-ui-react';
import { withTranslation } from 'react-i18next';
import FormField from '../../BaseComponents';
import { BIG_TEXT_TYPE, DATE_TYPE, SELECT_TYPE, TEXT_TYPE } from '../../../constants/columnTypes';
import { connect } from 'react-redux';
import { valuesListSelector } from '../../../ducks/lookup';

class CreateOrder extends Component {
    handleChangeSoldTo = (e, { name, value, ext }) => {
        const { valuesList, onChange } = this.props;
        const soldToItem = valuesList.find(item => item.value === value) || {};

        onChange(e, {
            name,
            value,
        });
        onChange(e, { name: 'clientName', value: ext.warehouseName });
        onChange(e, { name: 'deliveryAddress', value: ext.address });
        onChange(e, { name: 'deliveryWarehouseId', value: value ? value.deliveryWarehouseId : null });
    };

    handleChangeShippingWarehouseId = (e, { name, value, ext }) => {
        const { valuesList, onChange } = this.props;

        onChange(e, {
            name,
            value,
        });

        onChange(e, { name: 'shippingAddress', value: ext.address });
    };

    render() {
        const {
            form = {},
            onChange,
            isNotUniqueNumber,
            uniquenessNumberCheck,
            error,
            t,
        } = this.props;
        return (
            <Form className="tabs-card">
                <Grid>
                    <Grid.Row columns={4}>
                        <Grid.Column>
                            <FormField
                                name="orderNumber"
                                type={TEXT_TYPE}
                                isRequired
                                value={form['orderNumber']}
                                error={
                                    (isNotUniqueNumber && t('number_already_exists')) ||
                                    error['orderNumber']
                                }
                                onBlur={uniquenessNumberCheck}
                                onChange={onChange}
                            />
                        </Grid.Column>
                        <Grid.Column>
                            <FormField
                                name="clientOrderNumber"
                                type={TEXT_TYPE}
                                error={error['clientOrderNumber']}
                                value={form['clientOrderNumber']}
                                onChange={onChange}
                            />
                        </Grid.Column>
                        <Grid.Column>
                            <FormField
                                name="orderDate"
                                type={DATE_TYPE}
                                error={error['orderDate']}
                                value={form['orderDate']}
                                onChange={onChange}
                            />
                        </Grid.Column>
                        <Grid.Column>
                            <FormField
                                name="payer"
                                type={TEXT_TYPE}
                                error={error['payer']}
                                value={form['payer']}
                                onChange={onChange}
                            />
                        </Grid.Column>
                    </Grid.Row>
                    <Grid.Row columns={4}>
                        <Grid.Column>
                            <FormField
                                name="shippingWarehouseId"
                                type={SELECT_TYPE}
                                value={form['shippingWarehouseId']}
                                error={error['shippingWarehouseId']}
                                source="shippingWarehousesForOrderCreation"
                                onChange={this.handleChangeShippingWarehouseId}
                            />
                        </Grid.Column>
                        <Grid.Column>
                            <FormField
                                name="deliveryWarehouseId"
                                type={SELECT_TYPE}
                                value={form['deliveryWarehouseId']}
                                error={error['deliveryWarehouseId']}
                                source="warehouses"
                                onChange={onChange}
                            />
                        </Grid.Column>
                        <Grid.Column>
                            <FormField
                                name="clientName"
                                type={TEXT_TYPE}
                                isDisabled
                                error={error['clientName']}
                                value={form['clientName']}
                                onChange={onChange}
                            />
                        </Grid.Column>
                        <Grid.Column>
                            <FormField
                                name="deliveryDate"
                                type={DATE_TYPE}
                                value={form['deliveryDate']}
                                error={error['deliveryDate']}
                                onChange={onChange}
                            />
                        </Grid.Column>
                    </Grid.Row>
                    <Grid.Row columns={2}>
                        <Grid.Column>
                            <FormField
                                name="shippingAddress"
                                type={BIG_TEXT_TYPE}
                                isRequired
                                value={form['shippingAddress']}
                                error={error['shippingAddress']}
                                isDisabled
                                rows={2}
                                onChange={onChange}
                            />
                        </Grid.Column>
                        <Grid.Column>
                            <FormField
                                name="deliveryAddress"
                                type={BIG_TEXT_TYPE}
                                isRequired
                                value={form['deliveryAddress']}
                                error={error['deliveryAddress']}
                                isDisabled
                                rows={2}
                                onChange={onChange}
                            />
                        </Grid.Column>
                    </Grid.Row>
                </Grid>
            </Form>
        );
    }
}

const mapStateToProps = state => {
    return {
        valuesList: valuesListSelector(state, 'soldTo') || [],
    };
};

const mapDispatchToProps = () => {};

export default withTranslation()(
    connect(
        mapStateToProps,
        mapDispatchToProps,
    )(CreateOrder),
);
