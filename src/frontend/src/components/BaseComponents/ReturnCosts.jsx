import React, { useState, useEffect, useMemo } from 'react';
import { Button, Dropdown, Form, Grid, Icon, Input } from 'semantic-ui-react';
import { useTranslation } from 'react-i18next';
import ReturnCostsConstructor from "./ReturnCostsConstructor";

const ReturnCosts = ({ orders = [], value = [], onChange, name: fieldName }) => {
    const { t } = useTranslation();
    const ids = useMemo(() => {
        return value ? value.map(item => item.id) : [];
    }, [value]);

    const options = useMemo(
        () => {

            return orders.map(order => ({
                key: order.id,
                value: order.id,
                text: order.orderNumber,
            }));
        },
        [orders],
    );

    const handleChange = (e, { name, value: cost, index }) => {
        if (!value || !value.length) {
            onChange(e, {
                name: fieldName,
                value: [
                    {
                        [name]: cost,
                    },
                ],
            });
        } else {
            const list = [...value];
            list[index] = {
                ...list[index],
                [name]: cost,
            };
            onChange(e, {
                name: fieldName,
                value: [...list],
            });
        }
    };

    const handleAdd = () => {
        onChange(null, {
            name: fieldName,
            value: [
                ...(value && value.length
                    ? value
                    : [
                          {
                              id: null,
                              returnCostWithoutVAT: null,
                          },
                      ]),
                {
                    id: null,
                    returnCostWithoutVAT: null,
                },
            ],
        });
    };

    const handleDelete = (index) => {
        const list = [...value];
        list.splice(index, 1);
        onChange(null, {
            name: fieldName,
            value: [...list],
        });
    };

    return (
        <div className="return_cost_field">
            <Grid>
                {(value && value.length
                    ? value
                    : [
                          {
                              id: null,
                              returnCostWithoutVAT: null,
                          },
                      ]
                ).map((item, index) => (
                    <ReturnCostsConstructor
                        item={item}
                        index={index}
                        options={options.filter(order => !ids.includes(order.value) || order.value === item.id)}
                        t={t}
                        isAdd={((value && index === value.length - 1) || !(value && value.length)) && value.length !== options.length}
                        onChange={handleChange}
                        onDelete={handleDelete}
                        onAdd={handleAdd}
                    />
                ))}
            </Grid>
        </div>
    );
};

export default React.memo(ReturnCosts);
